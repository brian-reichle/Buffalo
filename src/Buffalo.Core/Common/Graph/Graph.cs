// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	sealed partial class Graph<TNode, TTransition>
	{
		Graph()
		{
			_states = new List<State>();
			_startStates = new List<State>();
			StartStates = new ReadOnlyCollection<State>(_startStates);
			States = new ReadOnlyCollection<State>(_states);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public ReadOnlyCollection<State> StartStates { get; }
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public ReadOnlyCollection<State> States { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly List<State> _startStates;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly List<State> _states;

		interface IDeletable
		{
			void Delete();
		}
	}
}
