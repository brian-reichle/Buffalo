// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;

namespace Buffalo.Core.Lexer.Configuration
{
	sealed class Config
	{
		public Config()
		{
			Manager = new ConfigManager();
			Settings = new List<ConfigSetting>();
			States = new List<ConfigState>();
			Tables = new List<ConfigTable>();
			TokenTypes = new List<string>();
		}

		public ConfigManager Manager { get; }
		public List<ConfigSetting> Settings { get; }
		public List<ConfigState> States { get; }
		public List<ConfigTable> Tables { get; }
		public List<string> TokenTypes { get; }

		public string TableResourceNameFormat { get; set; }
	}
}
