// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using Buffalo.Core.Common;
using Buffalo.Core.Lexer.Configuration;

namespace Buffalo.Core.Lexer
{
	sealed class LexerGenerator : ICodeGenerator
	{
		public bool Load(TextReader reader, IErrorReporter reporter, ICodeGeneratorEnv environment)
		{
			var wrapper = new ReporterWrapper(reporter);

			_loadStart = null;
			_parseDone = null;
			_calculationDone = null;
			_writeStart = null;
			_writeDone = null;

			_loadStart = DateTime.Now;

			var parser = new ConfigParser(wrapper);
			var config = parser.Parse(new ConfigScanner(reader.ReadToEnd()));
			_parseDone = DateTime.Now;

			if (wrapper.HasError)
			{
				return false;
			}

			config.TableResourceNameFormat = environment.GetResourceName(".{0}.table");

			SyntaxTreeDecorator.Decorate(config, wrapper);

			_calculationDone = DateTime.Now;

			if (wrapper.HasError)
			{
				_config = null;
				return false;
			}
			else
			{
				_config = config;
				return true;
			}
		}

		public void Write(TextWriter writer)
		{
			if (_config == null) throw new InvalidOperationException("Generator not been loaded yet");

			SetupGenerator();

			_writeStart = DateTime.Now;
			_generator.Write(writer);
			_writeDone = DateTime.Now;
		}

		public void ReportPerformance(IPerformanceReporter reporter)
		{
			PerfReporterHelper.AddPerfMetric(reporter, "Parse", _loadStart, _parseDone);
			PerfReporterHelper.AddPerfMetric(reporter, "Automation Calculation", _parseDone, _calculationDone);
			PerfReporterHelper.AddPerfMetric(reporter, "Table Construction", _writeSetupStart, _writeSetupDone);
			PerfReporterHelper.AddPerfMetric(reporter, "Write", _writeStart, _writeDone);
		}

		public IEnumerable<IAdditionalFile> AdditionalFiles
		{
			get
			{
				if (_config == null) throw new InvalidOperationException("Generator not been loaded yet");

				SetupGenerator();

				for (var i = 0; i < _config.Tables.Count; i++)
				{
					if (!string.IsNullOrEmpty(_config.Tables[i].Name))
					{
						yield return new LexerTableFile(i, _generator);
					}
				}

				if (_config.Manager.RenderScanGraph == GraphStyle.SVG)
				{
					foreach (var table in _config.Tables)
					{
						yield return new SVGScanGraphRenderer(_config, table);
					}
				}
			}
		}

		void SetupGenerator()
		{
			if (_generator == null)
			{
				_writeSetupStart = DateTime.Now;
				_generator = new CodeGen(_config);
				_writeSetupDone = DateTime.Now;
			}
		}

		DateTime? _loadStart;
		DateTime? _parseDone;
		DateTime? _calculationDone;

		DateTime? _writeSetupStart;
		DateTime? _writeSetupDone;
		DateTime? _writeStart;
		DateTime? _writeDone;

		Config _config;
		CodeGen _generator;
	}
}
