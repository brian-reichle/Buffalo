// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using Buffalo.Core.Common;
using Buffalo.Core.Parser.Configuration;

namespace Buffalo.Core.Parser
{
	sealed class ParserGenerator : ICodeGenerator
	{
		public bool Load(TextReader reader, IErrorReporter reporter, ICodeGeneratorEnv environment)
		{
			var wrapper = new ReporterWrapper(reporter);

			_loadStart = null;
			_parseDone = null;
			_calculationDone = null;
			_tableDone = null;

			_loadStart = DateTime.Now;

			var parser = new ConfigParser(wrapper);
			var config = parser.Parse(new ConfigScanner(reader.ReadToEnd()));
			_parseDone = DateTime.Now;

			if (wrapper.HasError)
			{
				_config = null;
				return false;
			}

			SyntaxTreeDecorator.Decorate(config, wrapper);
			_calculationDone = DateTime.Now;

			if (wrapper.HasError)
			{
				_config = null;
				return false;
			}

			_data = TableGenerator.GenerateTables(config);
			_tableDone = DateTime.Now;

			if (!config.Manager.SuppressTableEmbedding)
			{
				config.TableResourceName = environment.GetResourceName(ParserTableFile.Suffix);
			}

			_config = config;
			return true;
		}

		public void Write(TextWriter writer)
		{
			SetupGenerator();

			_writeStart = DateTime.Now;
			_generator.Write(writer);
			_writeDone = DateTime.Now;
		}

		public void ReportPerformance(IPerformanceReporter reporter)
		{
			PerfReporterHelper.AddPerfMetric(reporter, "Parse", _loadStart, _parseDone);
			PerfReporterHelper.AddPerfMetric(reporter, "Automation Construction", _parseDone, _calculationDone);
			PerfReporterHelper.AddPerfMetric(reporter, "Table Construction", _calculationDone, _tableDone);
			PerfReporterHelper.AddPerfMetric(reporter, "Write", _writeStart, _writeDone);
		}

		public IEnumerable<IAdditionalFile> AdditionalFiles
		{
			get
			{
				SetupGenerator();

				if (!string.IsNullOrEmpty(_config.TableResourceName))
				{
					SetupGenerator();
					yield return new ParserTableFile(_generator);
				}

				if (_config.Manager.RenderParseTable)
				{
					yield return new ParseTableGen(_config, _data);
				}

				if (_config.Manager.RenderParseGraph == GraphStyle.SVG)
				{
					yield return new SVGParseGraphRenderer(_config.Graph.Graph, _data.StateMap);
				}
			}
		}

		void SetupGenerator()
		{
			if (_config == null) throw new InvalidOperationException("Generator not been loaded yet");

			if (_generator == null)
			{
				_generator = new CodeGen(_config, _data);
			}
		}

		DateTime? _loadStart;
		DateTime? _parseDone;
		DateTime? _calculationDone;
		DateTime? _tableDone;

		DateTime? _writeStart;
		DateTime? _writeDone;

		Config _config;
		TableData _data;
		CodeGen _generator;
	}
}
