// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common.Test
{
	static class FARenderer
	{
		public static string Render<TNode, TTransition>(Graph<TNode, TTransition>.State initialState)
			=> new FARenderer<TNode, TTransition>().Render(new Graph<TNode, TTransition>.State[] { initialState }, 0);

		public static string Render<TNode, TTransition>(Graph<TNode, TTransition> graph)
			=> new FARenderer<TNode, TTransition>().Render(graph.StartStates, 0);

		public static string Render<TNode, TTransition>(Graph<TNode, TTransition>.State initialState, int width)
			=> new FARenderer<TNode, TTransition>().Render(new Graph<TNode, TTransition>.State[] { initialState }, width);

		public static string Render<TNode, TTransition>(Graph<TNode, TTransition> graph, int width)
			=> new FARenderer<TNode, TTransition>().Render(graph.StartStates, width);
	}
}
