// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using Buffalo.Core.Common;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	sealed class TableData
	{
		public Statistics Statistics { get; set; }
		public int TerminalMask { get; set; }
		public int TerminalColumns { get; set; }
		public bool NeedsTerminalMask { get; set; }
		public IDictionary<Segment, int> GotoMap { get; set; }
		public CompressedBlob Actions { get; set; }
		public IDictionary<Segment, int> TerminalMap { get; set; }
		public IDictionary<Graph.State, int> StateMap { get; set; }
		public IDictionary<Production, int> ReductionMap { get; set; }
	}
}
