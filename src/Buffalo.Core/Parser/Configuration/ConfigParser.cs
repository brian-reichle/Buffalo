// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigParser : AutoConfigParser
	{
		public ConfigParser(IErrorReporter reporter)
		{
			if (reporter == null) throw new ArgumentNullException(nameof(reporter));
			_reporter = reporter;
		}

		protected override ConfigTokenType GetTokenType(ConfigToken terminal) => terminal.Type;

		protected override Config Reduce_Config_1(Config configSeg, ConfigProduction productionSeg)
		{
			configSeg.Productions.Add(productionSeg);
			return configSeg;
		}

		protected override Config Reduce_Config_2(Config configSeg, ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return configSeg;
		}

		protected override Config Reduce_ConfigSettings_1(Config configSettingsSeg, ConfigSetting settingSeg)
		{
			configSettingsSeg.Settings.Add(settingSeg);
			return configSettingsSeg;
		}

		protected override Config Reduce_ConfigSettings_2(Config configSettingsSeg, ConfigUsing usingSeg)
		{
			configSettingsSeg.Usings.Add(usingSeg);
			return configSettingsSeg;
		}

		protected override Config Reduce_ConfigSettings_3(Config configSettingsSeg, ConfigEntryPoint entryPointSeg)
		{
			configSettingsSeg.EntryPoints.Add(entryPointSeg);
			return configSettingsSeg;
		}

		protected override Config Reduce_ConfigSettings_4(Config configSettingsSeg, ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return configSettingsSeg;
		}

		protected override Config Reduce_ConfigSettings_5(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return new Config();
		}

		protected override Config Reduce_ConfigSettings_6()
		{
			return new Config();
		}

		protected override ConfigUsing Reduce_Using_1(ConfigToken labelSeg, ConfigToken stringSeg)
		{
			return new ConfigUsing(labelSeg, stringSeg);
		}

		protected override ConfigUsing Reduce_Using_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return new ConfigUsing(null, null);
		}

		protected override ConfigEntryPoint Reduce_EntryPoint_1(ConfigToken nonTerminalSeg)
		{
			return new ConfigEntryPoint()
			{
				NonTerminal = nonTerminalSeg,
			};
		}

		protected override ConfigEntryPoint Reduce_EntryPoint_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return new ConfigEntryPoint();
		}

		protected override ConfigProduction Reduce_Production_1(ConfigToken nonTerminalSeg, ConfigToken productionTypeDefSeg, ConfigProduction ruleListSeg)
		{
			ruleListSeg.Target = nonTerminalSeg;
			ruleListSeg.TypeRef = productionTypeDefSeg;
			return ruleListSeg;
		}

		protected override ConfigProduction Reduce_Production_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return null;
		}

		protected override ConfigProduction Reduce_RuleList_1(ConfigProduction ruleListSeg, ConfigRule rule)
		{
			ruleListSeg.Rules.Add(rule);
			return ruleListSeg;
		}

		protected override ConfigProduction Reduce_RuleList_2(ConfigRule rule)
		{
			var production = new ConfigProduction();
			production.Rules.Add(rule);
			return production;
		}

		protected override ConfigRule Reduce_Rule_2(ConfigRule segmentListSeg, ConfigCommand commandSeg)
		{
			segmentListSeg.Command = commandSeg;
			return segmentListSeg;
		}

		protected override ConfigSegment Reduce_Segment_1(ConfigToken rawSegmentSeg)
		{
			return new ConfigSegment()
			{
				Token = rawSegmentSeg,
			};
		}

		protected override ConfigSegment Reduce_Segment_2(ConfigToken rawSegmentSeg, ConfigToken segmentModifierSeg)
		{
			return new ConfigSegment()
			{
				Token = rawSegmentSeg,
				Modifier = segmentModifierSeg,
			};
		}

		protected override ConfigRule Reduce_SegmentList_1(ConfigRule segmentListSeg, ConfigSegment segmentSeg)
		{
			segmentListSeg.Segments.Add(segmentSeg);
			return segmentListSeg;
		}

		protected override ConfigRule Reduce_SegmentList_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return new ConfigRule();
		}

		protected override ConfigRule Reduce_SegmentList_3()
		{
			return new ConfigRule();
		}

		protected override ConfigSetting Reduce_Setting_1(ConfigToken labelSeg, ConfigToken settingValueSeg)
		{
			return new ConfigSetting(labelSeg, settingValueSeg);
		}

		protected override ConfigSetting Reduce_Setting_2(ConfigToken labelSeg, ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return null;
		}

		protected override ConfigCommand Reduce_Command_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return null;
		}

		protected override ConfigCommand Reduce_CommandExpression_1(ConfigToken argumentValueSeg)
		{
			return new ConfigCommandArg() { Value = argumentValueSeg };
		}

		protected override ConfigCommand Reduce_CommandExpression_2(ConfigToken nullSeg)
		{
			return new ConfigCommandNull() { Value = nullSeg };
		}

		void ReportUnexpectedToken(ConfigToken token)
		{
			if (token.Type == ConfigTokenType.Error)
			{
				ReporterHelper.AddError(_reporter, token, token.Text);
			}
			else if (token.Type == ConfigTokenType.EOF)
			{
				ReporterHelper.AddError(_reporter, token, "Unexpected end of file.");
			}
			else
			{
				ReporterHelper.AddError(_reporter, token, "Unexpected Token.");
			}
		}

		readonly IErrorReporter _reporter;
	}
}
