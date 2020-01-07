// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	sealed class ReEmptyLanguage : ReElement
	{
		public static ReEmptyLanguage Instance { get; } = new ReEmptyLanguage();

		ReEmptyLanguage()
		{
		}

		public override ReElementKind Kind => ReElementKind.EmptyLanguage;
		public override bool MatchesEmptyString => false;

		public override void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState)
		{
		}
	}
}
