// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class SyntaxTreeDecorator
	{
		SyntaxTreeDecorator(Config config, IErrorReporter reporter)
		{
			_config = config;
			_reporter = reporter;
			_terminals = new Dictionary<string, Segment>();
			_nonTerminals = new Dictionary<string, Segment>();
			_nextRuleNum = new Dictionary<string, int>();
			_optionalProductions = new Dictionary<Segment, Segment>();
			_ntTypes = new Dictionary<Segment, ConfigUsing>();
		}

		public static void Decorate(Config config, IErrorReporter reporter)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));
			if (reporter == null) throw new ArgumentNullException(nameof(reporter));

			config.Manager.Reset();
			config.RuleLookup.Clear();

			var decorator = new SyntaxTreeDecorator(config, reporter);
			decorator.DecorateCore();
		}

		void DecorateCore()
		{
			ReadSettings();

			var productions = _config.Productions;
			var initial = productions.Count;

			PopulateUsings();

			for (var i = 0; i < initial; i++)
			{
				DecorateProduction(productions[i]);
			}

			if (productions.Count == 0)
			{
				_reporter.AddError(1, 1, 1, 1, "No productions defined.");
				return;
			}

			PopulateTopLevelSegments();
			DetectUnreachableNonTerminals();

			if (!VerifyProductionReferences())
			{
				return;
			}

			_config.UseErrorRecovery = _terminals.ContainsKey(Segment.Error.Name);
			_config.Graph = ParseGraph.ConstructGraph(_config);

			ValidateGraph();
		}

		void DecorateProduction(ConfigProduction cProduction)
		{
			var target = cProduction.Segment;

			if (_ntTypes.TryGetValue(target, out var cUsing))
			{
				cProduction.Using = cUsing;
			}

			if (!_nextRuleNum.TryGetValue(target.Name, out var nextRuleNum))
			{
				nextRuleNum = 1;
			}

			foreach (var cRule in cProduction.Rules)
			{
				DecorateRule(cProduction, cRule, nextRuleNum++);
			}

			_nextRuleNum[target.Name] = nextRuleNum;
		}

		void DecorateRule(ConfigProduction cProduction, ConfigRule cRule, int ruleNum)
		{
			var cSegments = cRule.Segments;
			ImmutableArray<Segment> segments;

			if (cSegments.Count == 0)
			{
				cRule.FromPos = cProduction.Target.FromPos;
				cRule.ToPos = cProduction.Target.ToPos;
				segments = ImmutableArray<Segment>.Empty;
			}
			else
			{
				var builder = ImmutableArray.CreateBuilder<Segment>(cSegments.Count);
				cRule.FromPos = cSegments[0].Token.FromPos;
				cRule.ToPos = cSegments[cSegments.Count - 1].Token.ToPos;

				for (var i = 0; i < cSegments.Count; i++)
				{
					var cSegment = cSegments[i];
					var segment = GetSegment(cSegment.Token, cSegment.Modifier);

					cSegment.Segment = segment;
					builder.Add(segment);
				}

				segments = builder.MoveToImmutable();
			}

			var production = new Production(cProduction.Segment, segments);
			cRule.Production = production;

			PopulateCommand(cProduction, cRule, ruleNum);

			if (!_config.RuleLookup.ContainsKey(production))
			{
				_config.RuleLookup.Add(production, cRule);
			}
			else
			{
				ReporterHelper.AddWarning(_reporter, cRule, "The production '{0}' has already been defined.", production);
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

		void PopulateUsings()
		{
			var usingLookup = new Dictionary<string, ConfigUsing>();

			foreach (var cUsing in _config.Usings)
			{
				var label = cUsing.Label.Text;

				if (!usingLookup.ContainsKey(label))
				{
					usingLookup.Add(label, cUsing);
				}
				else
				{
					ReporterHelper.AddError(_reporter, cUsing.Label, "Type reference already defined.");
				}
			}

			{
				var label = _config.Manager.TokenType;

				if (!string.IsNullOrEmpty(label) && usingLookup.TryGetValue(label, out var terminalType))
				{
					_config.TerminalType = terminalType;
				}
			}

			var pending = new List<Segment>();

			foreach (var cProduction in _config.Productions)
			{
				var name = cProduction.Target.Text;
				var segment = GetNonTerminal(name);
				cProduction.Segment = segment;

				if (cProduction.TypeRef == null)
				{
					pending.Add(segment);
				}
				else
				{
					if (!usingLookup.TryGetValue(cProduction.TypeRef.Text, out var cUsing))
					{
						ReporterHelper.AddError(_reporter, cProduction.TypeRef, "Unknown type reference.");
					}
					else if (!_ntTypes.TryGetValue(segment, out var cExistingUsing))
					{
						_ntTypes.Add(segment, cUsing);
					}
					else if (cUsing != cExistingUsing)
					{
						ReporterHelper.AddError(_reporter, cProduction.TypeRef, "This type conflicts with a previous definition of <{0}>.", name);
					}
				}
			}

			ConfigUsing defaultUsing = null;

			if (_config.TerminalType == null)
			{
				defaultUsing = CreateDefaultUsing(usingLookup);
				_config.Usings.Add(defaultUsing);
				_config.TerminalType = defaultUsing;
			}

			foreach (var segment in pending)
			{
				if (_ntTypes.ContainsKey(segment)) continue;

				if (defaultUsing == null)
				{
					defaultUsing = CreateDefaultUsing(usingLookup);
					_config.Usings.Add(defaultUsing);
				}

				_ntTypes.Add(segment, defaultUsing);
			}
		}

		static ConfigUsing CreateDefaultUsing(Dictionary<string, ConfigUsing> usingLookup)
		{
			var current = 0;
			var defaultUsingLabel = "Object";

			while (usingLookup.ContainsKey(defaultUsingLabel))
			{
				current++;
				defaultUsingLabel = "Object" + current.ToString(CultureInfo.InvariantCulture);
			}

			var pos = new CharPos(0, 1, 1);

			return new ConfigUsing(
				label: new ConfigToken(ConfigTokenType.Label, pos, pos, defaultUsingLabel),
				className: new ConfigToken(ConfigTokenType.String, pos, pos, "System.Object"));
		}

		bool VerifyProductionReferences()
		{
			var success = true;

			var ntReferences = new Dictionary<Segment, List<ConfigToken>>();
			var ntDefinitions = new Dictionary<Segment, List<ConfigToken>>();

			foreach (var entryPoint in _config.EntryPoints)
			{
				AddToList(ntReferences, entryPoint.Segment, entryPoint.NonTerminal);
			}

			foreach (var nonTerminal in _config.Productions)
			{
				AddToList(ntDefinitions, nonTerminal.Segment, nonTerminal.Target);

				foreach (var rule in nonTerminal.Rules)
				{
					foreach (var segment in rule.Segments)
					{
						if (segment.Token.Type == ConfigTokenType.NonTerminal)
						{
							AddToList(ntReferences, segment.Segment, segment.Token);
						}
					}
				}
			}

			foreach (var pair in ntDefinitions)
			{
				ntReferences.Remove(pair.Key);

				var list = pair.Value;

				for (var i = 1; i < list.Count; i++)
				{
					var token = list[i];
					ReporterHelper.AddWarning(_reporter, token, "The non-terminal <{0}> has already been defined.", token.Text);
				}
			}

			foreach (var pair in ntReferences)
			{
				foreach (var token in pair.Value)
				{
					ReporterHelper.AddError(_reporter, token, "The non-terminal <{0}> is not defined.", token.Text);
					success = false;
				}
			}

			return success;
		}

		void PopulateCommand(ConfigProduction production, ConfigRule rule, int ruleNum)
		{
			var command = rule.Command;

			if (command == null)
			{
				var targetName = production.Target.Text;
				var methodName = string.Format(CultureInfo.InvariantCulture, "Reduce_{0}_{1}", targetName, ruleNum);
				rule.Command = command = GetImplicitCommand(methodName, rule);
			}
			else
			{
				var validator = new CommandValidator(_reporter);
				validator.Validate(command, rule.Production);
			}

			SetCommandSegment(command, production.Segment);
		}

		ConfigCommandMethod GetImplicitCommand(string methodName, ConfigRule rule)
		{
			var method = new ConfigCommandMethod()
			{
				Name = new ConfigToken(ConfigTokenType.Label, rule.FromPos, rule.ToPos, methodName),
			};

			for (var i = 0; i < rule.Segments.Count; i++)
			{
				var segment = rule.Segments[i];
				var token = segment.Token;

				if (segment.Modifier == null || segment.Modifier.Type != ConfigTokenType.Bang)
				{
					ConfigCommand argument = new ConfigCommandArg()
					{
						Value = new ConfigToken(ConfigTokenType.ArgumentValue, token.FromPos, token.ToPos, i.ToString(CultureInfo.InvariantCulture)),
					};

					SetCommandSegment(argument, segment.Segment);

					method.Arguments.Add(argument);
				}
			}

			return method;
		}

		void ValidateGraph()
		{
			var hasAccept = false;
			var hasRRConflict = false;

			foreach (var state in _config.Graph.Graph.States)
			{
				foreach (var pair in GetCompleteProductions(state.Label))
				{
					if (pair.Value.Count != 1)
					{
						ReportReduceReduceConflict(_config, pair.Value, _reporter);
						hasRRConflict = true;
					}
					else
					{
						var production = pair.Value[0];

						if (production.Target.IsInitial)
						{
							hasAccept = true;
						}
					}
				}
			}

			if (!hasAccept && !hasRRConflict)
			{
				_reporter.AddError(1, 1, 1, 1, "All accept actions were optomised away.");
			}
		}

		static void ReportReduceReduceConflict(Config config, List<Production> productions, IErrorReporter reporter)
		{
			var rules = new ConfigRule[productions.Count];
			var starts = new int[productions.Count];

			for (var i = 0; i < rules.Length; i++)
			{
				var production = productions[i];

				if (production.Target.IsInitial)
				{
					starts[i] = -1;
				}
				else
				{
					var rule = config.RuleLookup[production];
					rules[i] = rule;
					starts[i] = rule.FromPos.Index;
				}
			}

			Array.Sort(starts, rules);

			string format;
			string val;

			if (rules[0] == null)
			{
				format = "{0} causes a reduce-accept conflict.";
				val = null;
			}
			else
			{
				format = "{0} causes a reduce-reduce conflict with {1}.";
				val = rules[0].Production.ToString();
			}

			for (var i = 1; i < rules.Length; i++)
			{
				ReporterHelper.AddError(reporter, rules[i], format, rules[i].Production, val);
			}
		}

		static Dictionary<Segment, List<Production>> GetCompleteProductions(ParseItemSet parseItems)
		{
			var result = new Dictionary<Segment, List<Production>>();

			foreach (var item in parseItems)
			{
				if (item.Position != item.Production.Segments.Length) continue;

				foreach (var lookahead in parseItems.GetLookahead(item))
				{
					if (!result.TryGetValue(lookahead, out var list))
					{
						list = new List<Production>();
						result.Add(lookahead, list);
					}

					if (!list.Contains(item.Production))
					{
						list.Add(item.Production);
					}
				}
			}

			return result;
		}

		static void AddToList<TKey, TValue>(IDictionary<TKey, List<TValue>> lookup, TKey key, TValue value)
		{
			if (!lookup.TryGetValue(key, out var list))
			{
				list = new List<TValue>();
				lookup.Add(key, list);
			}

			list.Add(value);
		}

		void SetCommandSegment(ConfigCommand command, Segment segment)
		{
			command.Segment = segment;

			if (segment.IsTerminal)
			{
				command.Using = _config.TerminalType;
			}
			else if (_ntTypes.TryGetValue(segment, out var cUsing))
			{
				command.Using = cUsing;
			}
		}

		void PopulateTopLevelSegments()
		{
			if (_config.EntryPoints.Count > 0)
			{
				var segments = new Dictionary<Segment, bool>();

				foreach (var entryPoint in _config.EntryPoints)
				{
					var segment = GetNonTerminal(entryPoint.NonTerminal.Text);
					entryPoint.Segment = segment;

					if (_ntTypes.TryGetValue(segment, out var cUsing))
					{
						entryPoint.Using = cUsing;
					}

					if (segments.ContainsKey(segment))
					{
						ReporterHelper.AddWarning(_reporter, entryPoint.NonTerminal, "The non-terminal <{0}> is already an entry point.", entryPoint.NonTerminal.Text);
					}
					else
					{
						segments.Add(segment, true);
					}
				}

				_config.TopLevelSegments = SegmentSet.New(segments.Keys);
			}
			else if (_config.Productions.Count > 0)
			{
				_config.TopLevelSegments = SegmentSet.New(new Segment[]
				{
					_config.Productions[0].Segment,
				});

				_config.EntryPoints.Add(new ConfigEntryPoint()
				{
					Segment = _config.Productions[0].Segment,
					NonTerminal = _config.Productions[0].Target,
					Using = _config.Productions[0].Using,
				});
			}
			else
			{
				_config.TopLevelSegments = SegmentSet.EmptySet;
			}
		}

		void DetectUnreachableNonTerminals()
		{
			var references = new Dictionary<Segment, List<Segment>>();

			foreach (var production in _config.Productions)
			{
				var list = new List<Segment>();

				if (!references.TryGetValue(production.Segment, out list))
				{
					list = new List<Segment>();
					references.Add(production.Segment, list);
				}

				foreach (var rule in production.Rules)
				{
					foreach (var seg in rule.Segments)
					{
						var segment = seg.Segment;

						if (!segment.IsTerminal)
						{
							list.Add(segment);
						}
					}
				}
			}

			var reachable = new Dictionary<Segment, bool>();
			var pending = new Queue<Segment>();

			foreach (var entryPoint in _config.EntryPoints)
			{
				var segment = entryPoint.Segment;

				if (!reachable.ContainsKey(segment))
				{
					pending.Enqueue(segment);
					reachable.Add(segment, true);
				}
			}

			while (pending.Count > 0)
			{
				var segment = pending.Dequeue();

				if (references.TryGetValue(segment, out var reach))
				{
					foreach (var next in reach)
					{
						if (!reachable.ContainsKey(next))
						{
							pending.Enqueue(next);
							reachable.Add(next, true);
						}
					}
				}
			}

			foreach (var production in _config.Productions)
			{
				if (!reachable.ContainsKey(production.Segment))
				{
					ReporterHelper.AddWarning(_reporter, production.Target, "The non-terminal <{0}> is not reachable.", production.Target.Text);
				}
			}
		}

		Segment GetSegment(ConfigToken token, ConfigToken modifier)
		{
			if (modifier == null)
			{
				return GetSegment(token);
			}
			else
			{
				switch (modifier.Type)
				{
					case ConfigTokenType.QuestionMark:
						return GetOptionalProduction(token);

					case ConfigTokenType.Bang:
						return GetSegment(token);

					default:
						throw new InvalidOperationException();
				}
			}
		}

		Segment GetSegment(ConfigToken token)
		{
			if (token.Type == ConfigTokenType.NonTerminal)
			{
				return GetNonTerminal(token.Text);
			}
			else
			{
				return GetTerminal(token.Text);
			}
		}

		Segment GetOptionalProduction(ConfigToken token)
		{
			var subSegment = GetSegment(token);

			if (!_optionalProductions.TryGetValue(subSegment, out var masterSegment))
			{
				ConfigUsing cUsing;

				masterSegment = subSegment.GetOptional();

				if (subSegment.IsTerminal)
				{
					if ((cUsing = _config.TerminalType) != null)
					{
						_ntTypes.Add(masterSegment, cUsing);
					}
				}
				else if (_ntTypes.TryGetValue(subSegment, out cUsing))
				{
					_ntTypes.Add(masterSegment, cUsing);
				}

				var rule1 = new ConfigRule()
				{
					Segments =
					{
						new ConfigSegment()
						{
							Segment = subSegment,
							Token = token,
						},
					},
					Production = new Production(masterSegment, ImmutableArray.Create(subSegment)),
					Command = new ConfigCommandArg()
					{
						Value = new ConfigToken(ConfigTokenType.ArgumentValue, token.FromPos, token.ToPos, "0"),
						Segment = subSegment,
						Using = cUsing,
					},
					FromPos = token.FromPos,
					ToPos = token.ToPos,
				};

				var rule2 = new ConfigRule()
				{
					Segments =
					{
					},
					Production = new Production(masterSegment, ImmutableArray<Segment>.Empty),
					Command = new ConfigCommandNull()
					{
						Value = new ConfigToken(ConfigTokenType.Null, token.FromPos, token.ToPos, "null"),
						Segment = subSegment,
						Using = cUsing,
					},
					FromPos = token.FromPos,
					ToPos = token.ToPos,
				};

				var cProduction = new ConfigProduction()
				{
					Segment = masterSegment,
					Target = new ConfigToken(ConfigTokenType.NonTerminal, token.FromPos, token.ToPos, masterSegment.Name),
					Using = cUsing,
					Rules =
					{
						rule1,
						rule2,
					},
				};

				_config.Productions.Add(cProduction);
				_config.RuleLookup.Add(rule1.Production, rule1);
				_config.RuleLookup.Add(rule2.Production, rule2);

				_optionalProductions.Add(subSegment, masterSegment);
			}

			return masterSegment;
		}

		Segment GetNonTerminal(string name)
		{
			if (!_nonTerminals.TryGetValue(name, out var result))
			{
				result = new Segment(name, false);
				_nonTerminals.Add(name, result);
			}

			return result;
		}

		Segment GetTerminal(string name)
		{
			if (!_terminals.TryGetValue(name, out var result))
			{
				if (name == Segment.Error.Name)
				{
					result = Segment.Error;
				}
				else
				{
					result = new Segment(name, true);
				}

				_terminals.Add(name, result);
			}

			return result;
		}

		readonly Config _config;
		readonly IErrorReporter _reporter;
		readonly Dictionary<string, Segment> _terminals;
		readonly Dictionary<string, Segment> _nonTerminals;
		readonly Dictionary<string, int> _nextRuleNum;
		readonly Dictionary<Segment, ConfigUsing> _ntTypes;
		readonly Dictionary<Segment, Segment> _optionalProductions;
	}
}
