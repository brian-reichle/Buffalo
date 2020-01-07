// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	abstract class ReElement
	{
		public abstract ReElementKind Kind { get; }
		public abstract bool MatchesEmptyString { get; }
		public abstract void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState);
	}
}
