// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	[DebuggerDisplay("Singleton = {Label}")]
	sealed class ReSingleton : ReElement
	{
		internal ReSingleton(CharSet set)
		{
			if (set == null) throw new ArgumentNullException(nameof(set));
			_label = set;
		}

		public CharSet Label => _label;
		public override ReElementKind Kind => ReElementKind.Singleton;
		public override bool MatchesEmptyString => false;

		public override void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState)
			=> graph.AddTransition(fromState, toState, Label);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly CharSet _label;
	}
}
