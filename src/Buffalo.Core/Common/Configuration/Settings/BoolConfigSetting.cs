// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	sealed class BoolConfigSetting : BaseSetting<bool>
	{
		protected override bool SetCore(IErrorReporter reporter, IToken valueToken)
		{
			var text = valueToken.Text;

			if (valueToken.Type == SettingTokenType.Label)
			{
				switch (text)
				{
					case "true":
						Value = true;
						return true;

					case "false":
						Value = false;
						return false;

					default:
						ReporterHelper.AddError(reporter, valueToken, "'{0}' is not a valid value.", text);
						return false;
				}
			}
			else
			{
				ReporterHelper.AddError(reporter, valueToken, "\"{0}\" is not a valid value.", text);
				return false;
			}
		}
	}
}
