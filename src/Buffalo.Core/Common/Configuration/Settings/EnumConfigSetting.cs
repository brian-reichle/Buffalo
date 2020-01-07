// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Core.Common
{
	sealed class EnumConfigSetting<T> : BaseSetting<T>
	{
		public EnumConfigSetting(string description)
		{
			_description = description;
		}

		protected override bool SetCore(IErrorReporter reporter, IToken valueToken)
		{
			var text = valueToken.Text;

			if (valueToken.Type == SettingTokenType.Label)
			{
				try
				{
					Value = (T)Enum.Parse(typeof(T), text);
					return true;
				}
				catch (ArgumentException)
				{
					ReporterHelper.AddError(reporter, valueToken, "'{0}' is not a valid {1}.", text, _description);
					return false;
				}
			}
			else
			{
				ReporterHelper.AddError(reporter, valueToken, "\"{0}\" is not a valid {1}.", text, _description);
				return false;
			}
		}

		readonly string _description;
	}
}
