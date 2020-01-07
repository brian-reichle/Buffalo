// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	sealed partial class Graph<TNode, TTransition>
	{
		public sealed class Transition : IDeletable
		{
			public Transition(Graph<TNode, TTransition> graph, State fromState, State toState, TTransition label)
			{
				Graph = graph;
				FromState = fromState;
				ToState = toState;
				Label = label;
			}

			public bool IsDeleted { get; private set; }
			public Graph<TNode, TTransition> Graph { get; }
			public TTransition Label { get; }
			public State FromState { get; }
			public State ToState { get; }

			void IDeletable.Delete()
			{
				IsDeleted = true;
			}
		}
	}
}
