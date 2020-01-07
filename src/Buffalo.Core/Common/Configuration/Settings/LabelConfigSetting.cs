// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	sealed class LabelConfigSetting : BaseSetting<string>
	{
		protected override bool SetCore(IErrorReporter reporter, IToken valueToken)
		{
			if (valueToken.Type == SettingTokenType.Label)
			{
				Value = valueToken.Text;
				return true;
			}

			ReporterHelper.AddError(reporter, valueToken, "'{0}' is not a valid value.", valueToken.Text);
			return false;
		}
	}
}
