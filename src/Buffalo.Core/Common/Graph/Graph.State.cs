// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace Buffalo.Core.Common
{
	sealed partial class Graph<TNode, TTransition>
	{
		interface IStateEd
		{
			List<Transition> ToTransitions { get; }
			List<Transition> FromTransitions { get; }
		}

		[DebuggerDisplay("StateId = {StateId}, Start = {IsStartState}")]
		public sealed class State : IDeletable, IStateEd, IComparable<State>
		{
			public State(Graph<TNode, TTransition> graph, bool isStartState, TNode label)
			{
				Graph = graph;
				IsStartState = isStartState;
				Label = label;
				_fromTransitions = new List<Transition>();
				_toTransitions = new List<Transition>();
				FromTransitions = new ReadOnlyCollection<Transition>(_fromTransitions);
				ToTransitions = new ReadOnlyCollection<Transition>(_toTransitions);
				_id = Interlocked.Increment(ref _nextId);
			}

			public TNode Label { get; }
			public bool IsStartState { get; }
			public Graph<TNode, TTransition> Graph { get; }
			public bool IsDeleted { get; private set; }
			public ReadOnlyCollection<Transition> FromTransitions { get; }
			public ReadOnlyCollection<Transition> ToTransitions { get; }

			public int CompareTo(State other) => _id.CompareTo(other._id);

			void IDeletable.Delete()
			{
				IsDeleted = true;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			List<Transition> IStateEd.ToTransitions => _toTransitions;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			List<Transition> IStateEd.FromTransitions => _fromTransitions;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			readonly List<Transition> _fromTransitions;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			readonly List<Transition> _toTransitions;

			readonly int _id;
			static int _nextId;
		}
	}
}
