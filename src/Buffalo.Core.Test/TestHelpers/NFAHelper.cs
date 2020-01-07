// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;
using Buffalo.Core.Lexer;

namespace Buffalo.Core.Test
{
	static class NFAHelper
	{
		public static NFASegment<NodeData, CharSet> CreateSimplePath(Graph<NodeData, CharSet>.Builder graphBuilder, char c)
		{
			return CreateSimplePath(graphBuilder, c, c, false);
		}

		public static NFASegment<NodeData, CharSet> CreateSimplePath(Graph<NodeData, CharSet>.Builder graphBuilder, char fromChar, char toChar)
		{
			return CreateSimplePath(graphBuilder, fromChar, toChar, false);
		}

		public static NFASegment<NodeData, CharSet> CreateSimplePath(Graph<NodeData, CharSet>.Builder graphBuilder, char c, bool invert)
		{
			return CreateSimplePath(graphBuilder, c, c, invert);
		}

		public static NFASegment<NodeData, CharSet> CreateSimplePath(Graph<NodeData, CharSet>.Builder graphBuilder, char fromChar, char toChar, bool invert)
		{
			var set = CharSet.New(new CharRange[] { new CharRange(fromChar, toChar) });

			if (invert)
			{
				set = CharSet.Universal.Subtract(set);
			}

			return CreateSimplePath<NodeData, CharSet>(graphBuilder, set);
		}

		public static NFASegment<TNode, TTransition> CreateSimplePath<TNode, TTransition>(Graph<TNode, TTransition>.Builder graphBuilder, TTransition set)
		{
			var fromState = graphBuilder.NewState(false, default);
			var toState = graphBuilder.NewState(false, default);

			graphBuilder.AddTransition(fromState, toState, set);

			return new NFASegment<TNode, TTransition>()
			{
				Builder = graphBuilder,
				FromState = fromState,
				ToState = toState,
			};
		}

		public static NFASegment<TNode, TTransition> Series<TNode, TTransition>(Graph<TNode, TTransition>.Builder graphBuilder, params NFASegment<TNode, TTransition>[] segments)
		{
			var segment = segments[0];

			var fromState = segment.FromState;
			var toState = segment.ToState;

			for (var i = 1; i < segments.Length; i++)
			{
				segment = segments[i];
				graphBuilder.AddTransition(toState, segment.FromState);
				toState = segment.ToState;
			}

			return new NFASegment<TNode, TTransition>()
			{
				Builder = graphBuilder,
				FromState = fromState,
				ToState = toState,
			};
		}

		public static NFASegment<TNode, TTransition> Parallell<TNode, TTransition>(Graph<TNode, TTransition>.Builder graphBuilder, params NFASegment<TNode, TTransition>[] segments)
		{
			var segment = new NFASegment<TNode, TTransition>()
			{
				Builder = graphBuilder,
				FromState = graphBuilder.NewState(false, default),
				ToState = graphBuilder.NewState(false, default),
			};

			for (var i = 0; i < segments.Length; i++)
			{
				graphBuilder.AddTransition(segment.FromState, segments[i].FromState);
				graphBuilder.AddTransition(segments[i].ToState, segment.ToState);
			}

			return segment;
		}

		public static NFASegment<TNode, TTransition> Repeat<TNode, TTransition>(Graph<TNode, TTransition>.Builder graphBuilder, NFASegment<TNode, TTransition> segment)
		{
			graphBuilder.AddTransition(segment.ToState, segment.FromState);
			return segment;
		}

		public static NFASegment<TNode, TTransition> Optional<TNode, TTransition>(Graph<TNode, TTransition>.Builder graphBuilder, NFASegment<TNode, TTransition> segment)
		{
			graphBuilder.AddTransition(segment.FromState, segment.ToState);
			return segment;
		}

		public static Graph<NodeData, TTransition>.Builder Graph<TTransition>(Graph<NodeData, TTransition>.Builder graphBuilder, params NFASegment<NodeData, TTransition>[] segments)
		{
			var startState = graphBuilder.NewState(true, new NodeData(0, null));

			for (var i = 0; i < segments.Length; i++)
			{
				var endState = graphBuilder.NewState(false, new NodeData(null, i));
				graphBuilder.AddTransition(startState, segments[i].FromState);
				graphBuilder.AddTransition(segments[i].ToState, endState);
			}

			return graphBuilder;
		}
	}
}
