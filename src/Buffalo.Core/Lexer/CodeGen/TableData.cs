// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using Buffalo.Core.Common;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	sealed class TableData
	{
		public int TableID { get; set; }
		public Statistics Statistics { get; set; }
		public IList<char> CharClassificationBoundries { get; set; }
		public IList<int> CharClassification { get; set; }
		public Dictionary<Graph.State, int> StateMap { get; set; }
		public CompressedBlob TransitionTable { get; set; }
	}
}
