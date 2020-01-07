// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;
using Moq;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer.Test
{
	static class ReUtils
	{
		public static ReElement NewDummy(char c)
		{
			var mock = new Mock<ReElement>(MockBehavior.Strict);
			mock
				.Setup(x => x.GenerateNFA(It.IsNotNull<Graph.Builder>(), It.IsNotNull<Graph.State>(), It.IsNotNull<Graph.State>()))
				.Callback((Graph.Builder g, Graph.State s1, Graph.State s2) => g.AddTransition(s1, s2, CharSet.New(c)));

			mock.Setup(x => x.Kind).Returns((ReElementKind)(-1));
			mock.Setup(x => x.MatchesEmptyString).Returns(false);

			return mock.Object;
		}

		public static Graph.State Build(ReElement element)
		{
			var graph = new Graph<NodeData, CharSet>.Builder();
			var fromState = graph.NewState(false, new NodeData());
			var toState = graph.NewState(false, new NodeData());

			element.GenerateNFA(graph, fromState, toState);

			return fromState;
		}
	}
}
