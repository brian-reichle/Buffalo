// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer.Configuration
{
	sealed class ConfigState
	{
		public ConfigState()
		{
			Rules = new List<ConfigRule>();
		}

		public ConfigToken Label { get; set; }
		public List<ConfigRule> Rules { get; }

		public int GraphIndex { get; set; }
		public Graph.State StartState { get; set; }
	}
}
