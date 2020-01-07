// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	interface IGraphLayoutInfo<TNode, TTransition>
	{
		void SetStatePosition(Graph<TNode, TTransition>.State state, int x, int y, int width, int height);
		void SetTransitionLabel(Graph<TNode, TTransition>.Transition transition, int x, int y, int width, int height);
		void SetTransitionPath(Graph<TNode, TTransition>.Transition transition, int[] x, int[] y);
	}
}
