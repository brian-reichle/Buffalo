// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigEntryPoint
	{
		public ConfigToken NonTerminal { get; set; }
		public ConfigUsing Using { get; set; }
		public Segment Segment { get; set; }
	}
}
