// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	interface IGraphLayoutSpec<TNode, TTransition>
	{
		int NodeSeparation { get; }
		int RankSeparation { get; }

		int NodeWidth(Graph<TNode, TTransition>.State state);
		int NodeHeight(Graph<TNode, TTransition>.State state);
		int LabelWidth(Graph<TNode, TTransition>.Transition transition);
		int LabelHeight(Graph<TNode, TTransition>.Transition transition);
	}
}
