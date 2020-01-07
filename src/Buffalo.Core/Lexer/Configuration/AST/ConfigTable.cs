// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer.Configuration
{
	sealed class ConfigTable
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public Graph Graph { get; set; }
	}
}
