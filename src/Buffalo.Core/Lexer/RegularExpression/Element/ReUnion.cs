// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	[DebuggerDisplay("Union (Count = {_elements.Length})")]
	sealed class ReUnion : ReElement
	{
		internal ReUnion(ImmutableArray<ReElement> elements)
		{
			if (elements == null) throw new ArgumentNullException(nameof(elements));
			if (elements.Length == 0) throw new ArgumentException("elements cannot be empty", nameof(elements));
			_elements = elements;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public IEnumerable<ReElement> Elements => _elements;

		public override ReElementKind Kind => ReElementKind.Union;

		public override bool MatchesEmptyString
		{
			get
			{
				foreach (var element in _elements)
				{
					if (element.MatchesEmptyString)
					{
						return true;
					}
				}

				return false;
			}
		}

		public override void GenerateNFA(Graph.Builder graph, Graph.State fromState, Graph.State toState)
		{
			for (var i = 0; i < _elements.Length; i++)
			{
				_elements[i].GenerateNFA(graph, fromState, toState);
			}
		}

		ImmutableArray<ReElement> _elements;
	}
}
