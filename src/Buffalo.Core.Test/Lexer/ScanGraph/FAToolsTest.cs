// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common.Test;
using Buffalo.Core.Test;
using NUnit.Framework;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;
using NFASegment = Buffalo.Core.Test.NFASegment<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class FAToolsTest
	{
		[Test]
		public void SubsetConstruction_MultipleEntrypoints()
		{
			var nfaBuilder = new Graph.Builder();

			NFASegment segment;

			var s1 = nfaBuilder.NewState(true, new NodeData(1, null));
			var s2 = nfaBuilder.NewState(true, new NodeData(2, null));

			var e1 = nfaBuilder.NewState(false, new NodeData(null, 1));
			var e2 = nfaBuilder.NewState(false, new NodeData(null, 2));

			segment = NFAHelper.Series(
				nfaBuilder,
				NFAHelper.CreateSimplePath(nfaBuilder, 'c'),
				NFAHelper.CreateSimplePath(nfaBuilder, 'a'),
				NFAHelper.CreateSimplePath(nfaBuilder, 't'));

			nfaBuilder.AddTransition(s1, segment.FromState);
			nfaBuilder.AddTransition(segment.ToState, e1);

			segment = NFAHelper.Repeat(
				nfaBuilder,
				NFAHelper.CreateSimplePath(
					nfaBuilder,
					CharSet.New(new CharRange[]
					{
						new CharRange('a', 'z'),
						new CharRange('A', 'Z'),
					})));

			nfaBuilder.AddTransition(s1, segment.FromState);
			nfaBuilder.AddTransition(s2, segment.FromState);
			nfaBuilder.AddTransition(segment.ToState, e2);

			var dfaGraph = FATools.CreateDfa(nfaBuilder.Graph);

			const string expected =
				"0 (S:S1) -- [A-Z,a-b,d-z] --> 1 (L:E2)\r\n" +
				"0 (S:S1) -- [c] --> 2 (L:E2)\r\n" +
				"3 (S:S2) -- [A-Z,a-z] --> 1 (L:E2)\r\n" +
				"1 (L:E2) -- [A-Z,a-z] --> 1 (L:E2)\r\n" +
				"2 (L:E2) -- [A-Z,b-z] --> 1 (L:E2)\r\n" +
				"2 (L:E2) -- [a] --> 4 (L:E2)\r\n" +
				"4 (L:E2) -- [t] --> 5 (L:E1)\r\n" +
				"4 (L:E2) -- [A-Z,a-s,u-z] --> 1 (L:E2)\r\n" +
				"5 (L:E1) -- [A-Z,a-z] --> 1 (L:E2)\r\n" +
				"";

			Assert.That(FARenderer.Render(dfaGraph), Is.EqualTo(expected));
		}

		[Test]
		public void SubsetConstruction_String()
		{
			var nfaBuilder = new Graph.Builder();

			NFAHelper.Graph(
				nfaBuilder,
				NFAHelper.Series(
					nfaBuilder,
					NFAHelper.CreateSimplePath(nfaBuilder, '"'),
					NFAHelper.Optional(
						nfaBuilder,
						NFAHelper.Repeat(
							nfaBuilder,
							NFAHelper.Parallell(
								nfaBuilder,
								NFAHelper.Series(
									nfaBuilder,
									NFAHelper.CreateSimplePath(nfaBuilder, '"'),
									NFAHelper.CreateSimplePath(nfaBuilder, '"')),
								NFAHelper.CreateSimplePath(nfaBuilder, '"', true)))),
					NFAHelper.CreateSimplePath(nfaBuilder, '"')));

			var dfaGraph = FATools.CreateDfa(nfaBuilder.Graph);

			const string expected =
				"0 (S:S0) -- [\"] --> 1\r\n" +
				"1 -- [\"] --> 2 (L:E0)\r\n" +
				"1 -- ![\"] --> 1\r\n" +
				"2 (L:E0) -- [\"] --> 1\r\n" +
				"";

			Assert.That(FARenderer.Render(dfaGraph), Is.EqualTo(expected));
		}

		[Test]
		public void SubsetConstruction_Label()
		{
			var nfaGraphBuilder = new Graph.Builder();

			var lead = new CharRange[] { new CharRange('A', 'Z'), new CharRange('a', 'z') };
			var tail = new CharRange[] { new CharRange('A', 'Z'), new CharRange('a', 'z'), new CharRange('0', '9'), new CharRange('_', '_') };

			NFAHelper.Graph(
				nfaGraphBuilder,
				NFAHelper.Series(
					nfaGraphBuilder,
					NFAHelper.CreateSimplePath(nfaGraphBuilder, CharSet.New(lead)),
					NFAHelper.Optional(
						nfaGraphBuilder,
						NFAHelper.Repeat(
							nfaGraphBuilder,
							NFAHelper.CreateSimplePath(nfaGraphBuilder, CharSet.New(tail))))));

			var dfaGraph = FATools.CreateDfa(nfaGraphBuilder.Graph);

			const string expected =
				"0 (S:S0) -- [A-Z,a-z] --> 1 (L:E0)\r\n" +
				"1 (L:E0) -- [0-9,A-Z,_,a-z] --> 1 (L:E0)\r\n" +
				"";

			Assert.That(FARenderer.Render(dfaGraph), Is.EqualTo(expected));
		}

		[Test]
		public void SubsetConstruction_Precidence()
		{
			var graphBuilder = new Graph.Builder();

			NFAHelper.Graph(
				graphBuilder,
				NFAHelper.Series<NodeData, CharSet>(
					graphBuilder,
					NFAHelper.CreateSimplePath(graphBuilder, '1'),
					NFAHelper.CreateSimplePath(graphBuilder, '2'),
					NFAHelper.CreateSimplePath(graphBuilder, '3'),
					NFAHelper.CreateSimplePath(graphBuilder, '4')),
				NFAHelper.Repeat(
					graphBuilder,
					NFAHelper.CreateSimplePath(graphBuilder, '0', '9')));

			var dfaGraph = FATools.CreateDfa(graphBuilder.Graph);

			const string expected =
				"0 (S:S0) -- [0,2-9] --> 1 (L:E1)\r\n" +
				"0 (S:S0) -- [1] --> 2 (L:E1)\r\n" +
				"1 (L:E1) -- [0-9] --> 1 (L:E1)\r\n" +
				"2 (L:E1) -- [0-1,3-9] --> 1 (L:E1)\r\n" +
				"2 (L:E1) -- [2] --> 3 (L:E1)\r\n" +
				"3 (L:E1) -- [0-2,4-9] --> 1 (L:E1)\r\n" +
				"3 (L:E1) -- [3] --> 4 (L:E1)\r\n" +
				"4 (L:E1) -- [4] --> 5 (L:E0)\r\n" +
				"4 (L:E1) -- [0-3,5-9] --> 1 (L:E1)\r\n" +
				"5 (L:E0) -- [0-9] --> 1 (L:E1)\r\n" +
				"";

			Assert.That(FARenderer.Render(dfaGraph), Is.EqualTo(expected));
		}

		[Test]
		public void SplitDistinctGraphs_MultiStart()
		{
			var chars = "012345678";

			var graphBuilder = new Graph.Builder();
			var states = new Graph.State[12];

			for (var i = 0; i < 12; i++)
			{
				int? startState;
				int? endState;

				if (i < 3)
				{
					startState = i;
					endState = null;
				}
				else if (i > 8)
				{
					startState = null;
					endState = i - 9;
				}
				else
				{
					startState = null;
					endState = null;
				}

				states[i] = graphBuilder.NewState(startState.HasValue, new NodeData(startState, endState));

				if (i >= 3)
				{
					graphBuilder.AddTransition(states[i - 3], states[i], CharSet.New(chars[i - 3]));
				}
			}

			graphBuilder.AddTransition(states[3], states[7], CharSet.New('X'));

			var expected1 =
				"0 (S:S2) -- [2] --> 1\r\n" +
				"1 -- [5] --> 2\r\n" +
				"2 -- [8] --> 3 (L:E2)\r\n";

			var expected2 =
				"0 (S:S1) -- [1] --> 1\r\n" +
				"2 (S:S0) -- [0] --> 3\r\n" +
				"1 -- [4] --> 4\r\n" +
				"3 -- [3] --> 5\r\n" +
				"3 -- [X] --> 4\r\n" +
				"4 -- [7] --> 6 (L:E1)\r\n" +
				"5 -- [6] --> 7 (L:E0)\r\n";

			var results = FATools.SplitDistinctGraphs(graphBuilder.Graph);

			Assert.AreEqual(2, results.Length);
			Assert.AreEqual(expected1, FARenderer.Render(results[0]));
			Assert.AreEqual(expected2, FARenderer.Render(results[1]));
		}

		[Test]
		public void SplitDistinctGraphs_SingleStart()
		{
			var chars = "012345678";

			var graphBuilder = new Graph.Builder();
			var startState = graphBuilder.NewState(true, new NodeData(0, null));
			var states = new Graph.State[chars.Length];

			for (var i = 0; i < states.Length; i++)
			{
				states[i] = graphBuilder.NewState(false, new NodeData(null, i));
				graphBuilder.AddTransition(startState, states[i]);
			}

			var inGraph = graphBuilder.Graph;
			var results = FATools.SplitDistinctGraphs(inGraph);

			Assert.That(results.Length, Is.EqualTo(1));
			Assert.That(results[0], Is.SameAs(inGraph));
		}

		[Test]
		public void ExtractAlphabet()
		{
			var graphBuilder = new Graph.Builder();
			var state1 = graphBuilder.NewState(true, new NodeData(0, null));
			var state2 = graphBuilder.NewState(false, new NodeData(null, 0));
			var state3 = graphBuilder.NewState(false, new NodeData(null, 1));

			graphBuilder.AddTransition(state1, state2);
			graphBuilder.AddTransition(state2, state3, CharSet.New(new CharRange[] { new CharRange('A', 'Z') }));
			graphBuilder.AddTransition(state3, state2, CharSet.New(new CharRange[] { new CharRange('M', 'N') }));

			const string expected =
				"![A-Z]\r\n" +
				"[A-L,O-Z]\r\n" +
				"[M-N]\r\n" +
				"";

			Assert.That(Renderer.Render(FATools.ExtractAlphabet(graphBuilder.Graph)), Is.EqualTo(expected));
		}
	}
}
