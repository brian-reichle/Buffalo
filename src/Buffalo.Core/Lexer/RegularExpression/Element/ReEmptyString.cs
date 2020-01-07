// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	sealed class ReEmptyString : ReElement
	{
		public static ReEmptyString Instance { get; } = new ReEmptyString();

		ReEmptyString()
		{
		}

		public override ReElementKind Kind => ReElementKind.EmptyString;
		public override bool MatchesEmptyString => true;

		public override void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState)
			=> graph.AddTransition(fromState, toState);
	}
}
