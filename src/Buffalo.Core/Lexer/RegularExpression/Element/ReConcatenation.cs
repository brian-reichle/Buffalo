// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	[DebuggerDisplay("Concat (Count = {_elements.Length})")]
	sealed class ReConcatenation : ReElement
	{
		internal ReConcatenation(ImmutableArray<ReElement> elements)
		{
			if (elements == null) throw new ArgumentNullException(nameof(elements));
			if (elements.Length == 0) throw new ArgumentException("elements cannot be empty", nameof(elements));
			_elements = elements;
		}

		public override ReElementKind Kind => ReElementKind.Concatenation;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public IEnumerable<ReElement> Elements => _elements;

		public override bool MatchesEmptyString
		{
			get
			{
				foreach (var element in _elements)
				{
					if (!element.MatchesEmptyString)
					{
						return false;
					}
				}

				return true;
			}
		}

		public override void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState)
		{
			var lastState = toState;

			for (var i = _elements.Length - 1; i > 0; i--)
			{
				var nextState = graph.NewState(false, new NodeData());
				_elements[i].GenerateNFA(graph, nextState, lastState);
				lastState = nextState;
			}

			_elements[0].GenerateNFA(graph, fromState, lastState);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly ImmutableArray<ReElement> _elements;
	}
}
