// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using Buffalo.Core.Common;

namespace Buffalo.Core.Lexer.Configuration
{
	sealed class ConfigParser : AutoConfigParser
	{
		public ConfigParser(IErrorReporter reporter)
		{
			if (reporter == null) throw new ArgumentNullException(nameof(reporter));
			_reporter = reporter;
		}

		protected override ConfigTokenType GetTokenType(ConfigToken terminal) => terminal.Type;

		protected override Config Reduce_Config_1(Config configSeg, ConfigState stateSeg)
		{
			configSeg.States.Add(stateSeg);
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

		protected override Config Reduce_ConfigSettings_2(Config configSettingsSeg, ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return configSettingsSeg;
		}

		protected override Config Reduce_ConfigSettings_3(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return new Config();
		}

		protected override Config Reduce_ConfigSettings_4()
		{
			return new Config();
		}

		protected override ConfigRule Reduce_Rule_1(ConfigToken regexSeg, ConfigToken labelSeg)
		{
			return new ConfigRule(regexSeg, labelSeg);
		}

		protected override ConfigRule Reduce_Rule_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return null;
		}

		protected override ConfigState Reduce_RuleList_1(ConfigState ruleListSeg, ConfigRule ruleSeg)
		{
			ruleListSeg.Rules.Add(ruleSeg);
			return ruleListSeg;
		}

		protected override ConfigState Reduce_RuleList_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return new ConfigState();
		}

		protected override ConfigState Reduce_RuleList_3()
		{
			return new ConfigState();
		}

		protected override ConfigSetting Reduce_Setting_1(ConfigToken labelSeg, ConfigToken settingValueSeg)
		{
			return new ConfigSetting(labelSeg, settingValueSeg);
		}

		protected override ConfigSetting Reduce_Setting_2(ConfigToken errorSeg)
		{
			ReportUnexpectedToken(errorSeg);
			return null;
		}

		protected override ConfigState Reduce_State_1(ConfigToken labelSeg, ConfigState ruleListSeg)
		{
			ruleListSeg.Label = labelSeg;
			return ruleListSeg;
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
