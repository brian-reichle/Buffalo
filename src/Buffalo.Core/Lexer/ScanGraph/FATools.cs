// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using Buffalo.Core.Common;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	static class FATools
	{
		/// <summary>
		/// Extract the list of distinct char sets from the passed in graph, such that every possible char
		/// will belong to exactly one set in the list and every char in a set will always be treated the
		/// same by the graph.
		/// </summary>
		/// <param name="graph">The graph to extract the alphabet from.</param>
		/// <returns>The alphabet of the graph.</returns>
		public static CharSet[] ExtractAlphabet(Graph graph)
		{
			var list = new List<CharSet>();
			var everythingElse = CharSet.Universal;

			foreach (var state in graph.States)
			{
				foreach (var transition in state.ToTransitions)
				{
					if (transition.Label != null)
					{
						var value = transition.Label;
						everythingElse = everythingElse.Subtract(value);
						SplitListOnCharSet(list, value);
					}
				}
			}

			if (!everythingElse.IsEmptySet)
			{
				list.Add(everythingElse);
			}

			return list.ToArray();
		}

		static void SplitListOnCharSet(List<CharSet> list, CharSet set)
		{
			for (var i = list.Count - 1; i >= 0; i--)
			{
				var existing = list[i];

				if (set.Intersects(existing))
				{
					var intersect = existing.Intersection(set);
					var existingOnly = existing.Subtract(intersect);
					var newOnly = set.Subtract(intersect);

					list[i] = intersect;
					if (!existingOnly.IsEmptySet)
					{
						list.Add(existingOnly);
					}

					set = newOnly;
				}
			}

			if (!set.IsEmptySet)
			{
				list.Add(set);
			}
		}

		/// <summary>
		/// Create a DFA to simulate the given NFA using the subset construction algorithm.
		/// </summary>
		/// <param name="nfaGraph">The NFA to simulate.</param>
		/// <returns>The newly created DFA.</returns>
		public static Graph CreateDfa(Graph nfaGraph)
		{
			if (nfaGraph == null) throw new ArgumentNullException(nameof(nfaGraph));

			var graph = SubsetConstruction(nfaGraph);
			graph = SubsetConstructionR(graph);
			graph = SubsetConstruction(graph);
			return graph;
		}

		static Graph SubsetConstruction(Graph inGraph)
		{
			var stateSetMapping = new Dictionary<OrderedSet<Graph.State>, Graph.State>();
			var stateSetQueue = new Queue<OrderedSet<Graph.State>>();
			var graphBuilder = new Graph.Builder();

			foreach (var pair in GetStartStateSets(inGraph))
			{
				var setClosure = EpsilonClosure(pair.Value);
				var endState = GetDominatingTerminalState(setClosure);
				stateSetMapping[setClosure] = graphBuilder.NewState(true, new NodeData(pair.Key, endState));
				stateSetQueue.Enqueue(setClosure);
			}

			var transitions = new Dictionary<OrderedSet<Graph.State>, Dictionary<OrderedSet<Graph.State>, CharSet>>();

			var alphabet = ExtractAlphabet(inGraph);

			while (stateSetQueue.Count > 0)
			{
				var s = stateSetQueue.Dequeue();
				var localTransitions = new Dictionary<OrderedSet<Graph.State>, CharSet>();

				foreach (var a in alphabet)
				{
					var t = EpsilonClosure(Move(s, a));

					if (t.Length > 0)
					{
						UnionAdd(localTransitions, t, a);

						if (!stateSetMapping.ContainsKey(t))
						{
							stateSetMapping.Add(t, graphBuilder.NewState(false, new NodeData(null, GetDominatingTerminalState(t))));
							stateSetQueue.Enqueue(t);
						}
					}
				}

				transitions.Add(s, localTransitions);
			}

			foreach (var pair1 in transitions)
			{
				var fromState = stateSetMapping[pair1.Key];

				foreach (var pair2 in pair1.Value)
				{
					var toState = stateSetMapping[pair2.Key];
					graphBuilder.AddTransition(fromState, toState, pair2.Value);
				}
			}

			return graphBuilder.Graph;
		}

		static Graph SubsetConstructionR(Graph inGraph)
		{
			var stateSetMapping = new Dictionary<OrderedSet<Graph.State>, Graph.State>();
			var stateSetQueue = new Queue<OrderedSet<Graph.State>>();

			var graphBuilder = new Graph.Builder();

			foreach (var pair in GetEndStateSets(inGraph))
			{
				var startState = GetDominatingStartState(pair.Value);
				stateSetMapping[pair.Value] = graphBuilder.NewState(startState.HasValue, new NodeData(startState, pair.Key));
				stateSetQueue.Enqueue(pair.Value);
			}

			var transitions = new Dictionary<OrderedSet<Graph.State>, Dictionary<OrderedSet<Graph.State>, CharSet>>();

			var alphabet = ExtractAlphabet(inGraph);

			while (stateSetQueue.Count > 0)
			{
				var s = stateSetQueue.Dequeue();
				var localTransitions = new Dictionary<OrderedSet<Graph.State>, CharSet>();

				foreach (var a in alphabet)
				{
					var t = MoveR(s, a);

					if (t.Length > 0)
					{
						UnionAdd(localTransitions, t, a);

						if (!stateSetMapping.ContainsKey(t))
						{
							stateSetMapping.Add(t, graphBuilder.NewState(false, new NodeData()));
							stateSetQueue.Enqueue(t);
						}
					}
				}

				transitions.Add(s, localTransitions);
			}

			foreach (var pair1 in transitions)
			{
				var toState = stateSetMapping[pair1.Key];

				foreach (var pair2 in pair1.Value)
				{
					var fromState = stateSetMapping[pair2.Key];
					graphBuilder.AddTransition(fromState, toState, pair2.Value);
				}
			}

			var startStates = new Dictionary<int, Graph.State>();

			foreach (var pair in stateSetMapping)
			{
				foreach (var state in pair.Key)
				{
					if (!state.Label.StartState.HasValue) continue;

					var id = state.Label.StartState.Value;

					if (!startStates.TryGetValue(id, out var startState))
					{
						startState = graphBuilder.NewState(true, new NodeData(id, null));
						startStates.Add(id, startState);
					}

					graphBuilder.AddTransition(startState, pair.Value, null);
				}
			}

			return graphBuilder.Graph;
		}

		public static Graph[] SplitDistinctGraphs(Graph inGraph)
		{
			if (inGraph == null || inGraph.StartStates.Count == 0)
			{
				return Array.Empty<Graph>();
			}
			else if (inGraph.StartStates.Count == 1)
			{
				return new Graph[] { inGraph };
			}
			else
			{
				var result = new List<Graph>(inGraph.StartStates.Count);
				var startStates = new List<Graph.State>(inGraph.StartStates);

				var queue = new Queue<Graph.State>();
				var map = new Dictionary<Graph.State, Graph.State>();

				while (startStates.Count > 0)
				{
					map.Clear();

					var graphBuilder = new Graph.Builder();
					result.Add(graphBuilder.Graph);

					var oldState = startStates[startStates.Count - 1];
					startStates.RemoveAt(startStates.Count - 1);

					var newState = graphBuilder.NewState(oldState.IsStartState, oldState.Label);
					map.Add(oldState, newState);
					queue.Enqueue(oldState);

					while (queue.Count > 0)
					{
						oldState = queue.Dequeue();
						newState = map[oldState];

						foreach (var transition in oldState.ToTransitions)
						{
							var oldToState = transition.ToState;

							if (!map.TryGetValue(oldToState, out var newToState))
							{
								newToState = graphBuilder.NewState(oldToState.IsStartState, oldToState.Label);
								map.Add(oldToState, newToState);
								queue.Enqueue(oldToState);

								if (oldToState.IsStartState)
								{
									startStates.Remove(oldToState);
								}
							}

							graphBuilder.AddTransition(newState, newToState, transition.Label);
						}

						foreach (var transition in oldState.FromTransitions)
						{
							var oldFromState = transition.FromState;

							if (!map.ContainsKey(oldFromState))
							{
								var newFromState = graphBuilder.NewState(oldFromState.IsStartState, oldFromState.Label);
								map.Add(oldFromState, newFromState);
								queue.Enqueue(oldFromState);

								if (oldFromState.IsStartState)
								{
									startStates.Remove(oldFromState);
								}
							}
						}
					}
				}

				return result.ToArray();
			}
		}

		static List<KeyValuePair<int, OrderedSet<Graph.State>>> GetEndStateSets(Graph graph)
		{
			var states = new List<Graph.State>(graph.States.Count);

			foreach (var state in graph.States)
			{
				if (state.Label.EndState.HasValue)
				{
					states.Add(state);
				}
			}

			var result = new List<KeyValuePair<int, OrderedSet<Graph.State>>>();

			if (states.Count > 0)
			{
				states.Sort((s1, s2) => s1.Label.EndState.Value - s2.Label.EndState.Value);

				var currentLabel = states[0].Label.EndState.Value;
				var currentStates = new List<Graph.State>();

				foreach (var state in states)
				{
					if (state.Label.EndState != currentLabel)
					{
						result.Add(new KeyValuePair<int, OrderedSet<Graph.State>>(currentLabel, OrderedSet<Graph.State>.New(currentStates)));
						currentStates.Clear();
						currentLabel = state.Label.EndState.Value;
					}

					currentStates.Add(state);
				}

				result.Add(new KeyValuePair<int, OrderedSet<Graph.State>>(currentLabel, OrderedSet<Graph.State>.New(currentStates)));
			}

			return result;
		}

		static List<KeyValuePair<int, OrderedSet<Graph.State>>> GetStartStateSets(Graph graph)
		{
			var states = new List<Graph.State>(graph.States.Count);

			foreach (var state in graph.States)
			{
				if (state.Label.StartState.HasValue)
				{
					states.Add(state);
				}
			}

			var result = new List<KeyValuePair<int, OrderedSet<Graph.State>>>();

			if (states.Count > 0)
			{
				states.Sort((s1, s2) => s1.Label.StartState.Value - s2.Label.StartState.Value);

				var currentLabel = states[0].Label.StartState.Value;
				var currentStates = new List<Graph.State>();

				foreach (var state in states)
				{
					if (state.Label.StartState != currentLabel)
					{
						result.Add(new KeyValuePair<int, OrderedSet<Graph.State>>(currentLabel, OrderedSet<Graph.State>.New(currentStates)));
						currentStates.Clear();
						currentLabel = state.Label.StartState.Value;
					}

					currentStates.Add(state);
				}

				result.Add(new KeyValuePair<int, OrderedSet<Graph.State>>(currentLabel, OrderedSet<Graph.State>.New(currentStates)));
			}

			return result;
		}

		static int? GetDominatingStartState(OrderedSet<Graph.State> states)
		{
			foreach (var state in states)
			{
				if (state.Label.StartState.HasValue)
				{
					return state.Label.StartState.Value;
				}
			}

			return null;
		}

		static int? GetDominatingTerminalState(OrderedSet<Graph.State> states)
		{
			int? result = null;
			var priority = -1;

			foreach (var state in states)
			{
				if (state.Label.EndState != null)
				{
					if (result == null || state.Label.Priority < priority)
					{
						result = state.Label.EndState;
						priority = state.Label.Priority;
					}
				}
			}

			return result;
		}

		static OrderedSet<Graph.State> Move(OrderedSet<Graph.State> initialStates, CharSet c)
		{
			var reachable = new Dictionary<Graph.State, bool>();

			foreach (var state in initialStates)
			{
				foreach (var transition in state.ToTransitions)
				{
					if (transition.Label == null) continue;
					if (!transition.Label.Intersects(c)) continue;

					reachable[transition.ToState] = true;
				}
			}

			var result = new Graph.State[reachable.Count];
			reachable.Keys.CopyTo(result, 0);
			return OrderedSet<Graph.State>.New(result);
		}

		static OrderedSet<Graph.State> MoveR(OrderedSet<Graph.State> initialStates, CharSet c)
		{
			var reachable = new Dictionary<Graph.State, bool>();

			foreach (var state in initialStates)
			{
				foreach (var transition in state.FromTransitions)
				{
					if (transition.Label == null) continue;
					if (!transition.Label.Intersects(c)) continue;

					reachable[transition.FromState] = true;
				}
			}

			var result = new Graph.State[reachable.Count];
			reachable.Keys.CopyTo(result, 0);
			return OrderedSet<Graph.State>.New(result);
		}

		static OrderedSet<Graph.State> EpsilonClosure(OrderedSet<Graph.State> initialStates)
		{
			var reached = new Dictionary<Graph.State, bool>();
			var statesToExamine = new Queue<Graph.State>();

			foreach (var state in initialStates)
			{
				statesToExamine.Enqueue(state);
				reached.Add(state, true);
			}

			while (statesToExamine.Count > 0)
			{
				var state = statesToExamine.Dequeue();

				foreach (var transition in state.ToTransitions)
				{
					if (transition.Label != null) continue;
					if (reached.ContainsKey(transition.ToState)) continue;

					reached.Add(transition.ToState, true);
					statesToExamine.Enqueue(transition.ToState);
				}
			}

			var result = new Graph.State[reached.Count];
			reached.Keys.CopyTo(result, 0);
			return OrderedSet<Graph.State>.New(result);
		}

		static void UnionAdd(IDictionary<OrderedSet<Graph.State>, CharSet> transitions, OrderedSet<Graph.State> states, CharSet symbols)
		{
			if (transitions.TryGetValue(states, out var existingSymbols))
			{
				symbols = symbols.Union(existingSymbols);
			}

			transitions[states] = symbols;
		}
	}
}
