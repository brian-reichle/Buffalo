// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;

namespace Buffalo.Core.Common
{
	abstract class ConfigSettingList
	{
		protected void AddSetting(string name, IConfigSetting setting)
		{
			_lookup.Add(name, setting);
		}

		public void Reset()
		{
			_seenOptions.Clear();

			foreach (var setting in _lookup.Values)
			{
				setting.Reset();
			}
		}

		public void Set(IErrorReporter reporter, IToken labelToken, IToken valueToken)
		{
			var labelText = labelToken.Text;

			if (_lookup.TryGetValue(labelText, out var setting))
			{
				var isValueValid = setting.Set(reporter, valueToken);

				if (isValueValid)
				{
					if (_seenOptions.ContainsKey(labelText))
					{
						ReporterHelper.AddWarning(reporter, labelToken, "{0} has already been defined.", labelText);
					}
					else
					{
						_seenOptions.Add(labelText, true);
					}
				}
			}
			else
			{
				ReporterHelper.AddError(reporter, labelToken, "'{0}' is not a recognised option.", labelText);
			}
		}

		readonly Dictionary<string, bool> _seenOptions = new Dictionary<string, bool>();
		readonly Dictionary<string, IConfigSetting> _lookup = new Dictionary<string, IConfigSetting>();
	}
}
