// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigSetting
	{
		public ConfigSetting(ConfigToken label, ConfigToken value)
		{
			Label = label;
			Value = value;
		}

		public ConfigToken Label { get; }
		public ConfigToken Value { get; }
	}
}
