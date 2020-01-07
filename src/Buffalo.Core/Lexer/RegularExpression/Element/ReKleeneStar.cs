// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	[DebuggerDisplay("Kleene Star")]
	sealed class ReKleeneStar : ReElement
	{
		internal ReKleeneStar(ReElement element)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));
			_element = element;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReElement Element => _element;
		public override ReElementKind Kind => ReElementKind.KleenStar;
		public override bool MatchesEmptyString => true;

		public override void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState)
		{
			var statei = graph.NewState(false, new NodeData());

			_element.GenerateNFA(graph, statei, statei);
			graph.AddTransition(statei, toState);
			graph.AddTransition(fromState, statei);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ReElement _element;
	}
}
