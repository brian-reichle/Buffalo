// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	[DebuggerDisplay("Instance = {InstanceID}")]
	sealed partial class Graph<TNode, TTransition>
	{
		public sealed class Builder
		{
			public Graph<TNode, TTransition> Graph { get; } = new Graph<TNode, TTransition>();

			public State NewState(bool isStartState, TNode label)
			{
				var state = new State(Graph, isStartState, label);
				Graph._states.Add(state);

				if (isStartState)
				{
					Graph._startStates.Add(state);
				}

				return state;
			}

			public void AddTransition(State fromState, State toState)
				=> AddTransition(fromState, toState, default);

			public void AddTransition(State fromState, State toState, TTransition label)
			{
				if (fromState == null) throw new ArgumentNullException(nameof(fromState));
				if (toState == null) throw new ArgumentNullException(nameof(toState));

				if (fromState.Graph != Graph) throw new ArgumentException("state1 does not belong to this graph", nameof(fromState));
				if (toState.Graph != Graph) throw new ArgumentException("state2 does not belong to this graph", nameof(toState));

				AddTransitionCore(fromState, toState, label);
			}

			public void Delete(State state)
			{
				if (state == null) throw new ArgumentNullException(nameof(state));
				if (state.Graph != Graph) throw new ArgumentException("state does not belong to this graph", nameof(state));

				foreach (var transition in state.FromTransitions)
				{
					((IStateEd)transition.FromState).ToTransitions.Remove(transition);
					((IDeletable)transition).Delete();
				}

				foreach (var transition in state.ToTransitions)
				{
					((IStateEd)transition.ToState).FromTransitions.Remove(transition);
					((IDeletable)transition).Delete();
				}

				if (state.IsStartState)
				{
					Graph._startStates.Remove(state);
				}

				Graph._states.Remove(state);

				((IDeletable)state).Delete();
			}

			public void Delete(Transition transition)
			{
				if (transition == null) throw new ArgumentNullException(nameof(transition));
				if (transition.Graph != Graph) throw new ArgumentException("transition does not belong to this graph", nameof(transition));

				((IStateEd)transition.FromState).ToTransitions.Remove(transition);
				((IStateEd)transition.ToState).FromTransitions.Remove(transition);
				((IDeletable)transition).Delete();
			}

			void AddTransitionCore(State fromState, State toState, TTransition label)
			{
				var transaction = new Transition(Graph, fromState, toState, label);
				((IStateEd)fromState).ToTransitions.Add(transaction);
				((IStateEd)toState).FromTransitions.Add(transaction);
			}
		}
	}
}
