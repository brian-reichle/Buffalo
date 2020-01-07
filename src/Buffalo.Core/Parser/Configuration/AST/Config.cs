// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class Config
	{
		public Config()
		{
			Manager = new ConfigManager();
			Settings = new List<ConfigSetting>();
			EntryPoints = new List<ConfigEntryPoint>();
			Usings = new List<ConfigUsing>();
			Productions = new List<ConfigProduction>();
			RuleLookup = new Dictionary<Production, ConfigRule>();
		}

		public bool UseErrorRecovery { get; set; }
		public string TableResourceName { get; set; }
		public SegmentSet TopLevelSegments { get; set; }
		public ConfigUsing TerminalType { get; set; }
		public ParseGraph Graph { get; set; }
		public ConfigManager Manager { get; }
		public Dictionary<Production, ConfigRule> RuleLookup { get; }
		public List<ConfigSetting> Settings { get; }
		public List<ConfigUsing> Usings { get; }
		public List<ConfigEntryPoint> EntryPoints { get; }
		public List<ConfigProduction> Productions { get; }
	}
}
