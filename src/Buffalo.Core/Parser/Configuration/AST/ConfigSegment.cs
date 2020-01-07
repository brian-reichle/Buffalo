// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigSegment
	{
		public Segment Segment { get; set; }
		public ConfigToken Token { get; set; }
		public ConfigToken Modifier { get; set; }
	}
}
