// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigRule : ICharRange
	{
		public ConfigRule()
		{
			Segments = new List<ConfigSegment>();
		}

		public Production Production { get; set; }
		public CharPos FromPos { get; set; }
		public CharPos ToPos { get; set; }

		public ConfigCommand Command { get; set; }
		public List<ConfigSegment> Segments { get; }
	}
}
