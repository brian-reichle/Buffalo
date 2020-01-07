// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using Buffalo.Core.Common;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer.Configuration
{
	sealed class SyntaxTreeDecorator
	{
		SyntaxTreeDecorator(Config config, IErrorReporter reporter)
		{
			_config = config;
			_reporter = reporter;
		}

		public static void Decorate(Config config, IErrorReporter reporter)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));
			if (reporter == null) throw new ArgumentNullException(nameof(reporter));

			config.Manager.Reset();

			var decorator = new SyntaxTreeDecorator(config, reporter);
			decorator.DecorateCore();
		}

		void DecorateCore()
		{
			ReadSettings();

			if (_config.States.Count == 0)
			{
				_reporter.AddError(1, 1, 1, 1, "No states defined.");
				return;
			}

			GenerateGraphs();
			PopulateTables();
			PopulateEntryPoints();
		}

		void GenerateGraphs()
		{
			var typeLookup = new Dictionary<string, int>();
			_graphs = FATools.SplitDistinctGraphs(BuildDFA(typeLookup));
			_config.TokenTypes.AddRange(AssembleTokenTypeList(typeLookup));
		}

		void PopulateTables()
		{
			string format;

			if (_config.Manager.SuppressTableEmbedding)
			{
				format = null;
			}
			else
			{
				format = _config.TableResourceNameFormat;
			}

			for (var i = 0; i < _graphs.Length; i++)
			{
				var table = new ConfigTable()
				{
					Index = i,
					Graph = _graphs[i],
					Name = format == null ? null : string.Format(CultureInfo.InvariantCulture, format, i),
				};

				_config.Tables.Add(table);
			}
		}

		void PopulateEntryPoints()
		{
			foreach (var table in _config.Tables)
			{
				foreach (var state in table.Graph.StartStates)
				{
					var cStateIndex = state.Label.StartState.Value;

					var cState = _config.States[cStateIndex];
					cState.GraphIndex = table.Index;
					cState.StartState = state;
				}
			}
		}

		void ReadSettings()
		{
			var manager = _config.Manager;

			foreach (var cSetting in _config.Settings)
			{
				manager.Set(_reporter, cSetting.Label, cSetting.Value);
			}
		}

		Graph BuildDFA(Dictionary<string, int> typeLookup)
		{
			var nfa = new Graph<NodeData, CharSet>.Builder();

			for (var i = 0; i < _config.States.Count; i++)
			{
				var state = _config.States[i];

				if (state.Rules.Count == 0)
				{
					ReporterHelper.AddError(_reporter, state.Label, "The state '{0}' does not define any rules.", state.Label.Text);
					continue;
				}

				var startState = nfa.NewState(true, new NodeData(i, null));

				for (var j = 0; j < state.Rules.Count; j++)
				{
					var rule = state.Rules[j];
					ReElement element;

					if (!typeLookup.TryGetValue(rule.Token.Text, out var endStateID))
					{
						endStateID = typeLookup.Count;
						typeLookup.Add(rule.Token.Text, endStateID);
					}

					try
					{
						element = ReParser.Parse(rule.Regex.Text);
					}
					catch (ReParseException ex)
					{
						ReporterHelper.AddError(_reporter, rule.Regex, ex.Message);
						continue;
					}

					if (element.MatchesEmptyString)
					{
						ReporterHelper.AddWarning(
							_reporter,
							rule.Regex,
							"This regular expression claims to match the empty string, " +
							"this is not supported and usually indicates a typeo in the regular expression");
					}

					element.GenerateNFA(nfa, startState, nfa.NewState(false, new NodeData(null, endStateID, j)));
				}
			}

			return FATools.CreateDfa(nfa.Graph);
		}

		static string[] AssembleTokenTypeList(Dictionary<string, int> typeLookup)
		{
			var result = new string[typeLookup.Count];

			foreach (var pair in typeLookup)
			{
				result[pair.Value] = pair.Key;
			}

			return result;
		}

		Graph[] _graphs;
		readonly Config _config;
		readonly IErrorReporter _reporter;
	}
}
