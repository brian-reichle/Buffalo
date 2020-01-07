// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;

namespace Buffalo.Core.Test
{
	class NFASegment<TNode, TTransition>
	{
		public Graph<TNode, TTransition>.Builder Builder { get; set; }
		public Graph<TNode, TTransition>.State FromState { get; set; }
		public Graph<TNode, TTransition>.State ToState { get; set; }
	}
}
