// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigProduction
	{
		public ConfigProduction()
		{
			Rules = new List<ConfigRule>();
		}

		public Segment Segment { get; set; }
		public ConfigToken Target { get; set; }
		public ConfigToken TypeRef { get; set; }
		public ConfigUsing Using { get; set; }
		public List<ConfigRule> Rules { get; }
	}
}
