// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using Buffalo.Core.Parser.Configuration;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	sealed class ParseGraphOptimiser
	{
		public static void Optimise(Config config, Graph.Builder graph)
		{
			new ParseGraphOptimiser(config, graph).OptimiseCore();
		}

		ParseGraphOptimiser(Config config, Graph.Builder graph)
		{
			_config = config;
			_graph = graph;
			_trivialLookup = ExtractTrivialProductions(config);
		}

		void OptimiseCore()
		{
			if (_config.Manager.TrimParseGraph)
			{
				InlineTrivialReductions();
				StripImpossibleLookAheads();
				StripImpossibleGotos();
				StripUnreachableStates();
			}
			else
			{
				StripImpossibleLookAheads();
			}
		}

		void InlineTrivialReductions()
		{
			foreach (var state in _graph.Graph.States)
			{
				var items = FindTrivialItems(state.Label);
				if (items == null) continue;

				var mappings = CalculateTargetMappings(items);

				foreach (var item in items)
				{
					var production = item.Production;
					var nextSegment = production.Segments[0];

					if (!mappings.TryGetValue(production.Target, out var target))
					{
						target = production.Target;
					}

					var newTransition = GetTransition(state, target);
					var newTarget = newTransition.ToState;

					_graph.AddTransition(state, newTarget, nextSegment);
					_graph.Delete(GetTransition(state, nextSegment));
				}
			}
		}

		void StripUnreachableStates()
		{
			var graph = _graph.Graph;
			var reachable = new Dictionary<Graph.State, bool>();
			var pending = new Queue<Graph.State>();

			foreach (var state in graph.StartStates)
			{
				reachable.Add(state, true);
				pending.Enqueue(state);
			}

			while (pending.Count > 0)
			{
				var state = pending.Dequeue();

				foreach (var transition in state.ToTransitions)
				{
					var nextState = transition.ToState;

					if (!reachable.ContainsKey(nextState))
					{
						reachable.Add(nextState, true);
						pending.Enqueue(nextState);
					}
				}
			}

			var i = graph.States.Count;

			while (i > 0)
			{
				i--;

				var state = graph.States[i];

				if (!reachable.ContainsKey(state))
				{
					_graph.Delete(state);
				}
			}
		}

		void StripImpossibleLookAheads()
		{
			var segments = new List<Segment>();

			foreach (var state in _graph.Graph.States)
			{
				segments.Clear();

				foreach (var transition in state.ToTransitions)
				{
					if (!transition.Label.IsTerminal) continue;
					segments.Add(transition.Label);
				}

				if (segments.Count > 0)
				{
					state.Label.SubtractLookaheads(SegmentSet.New(segments));
				}
			}
		}

		void StripImpossibleGotos()
		{
			var reachableTransitions = CalculatePopulateReachability();
			var transitionsToDelete = new List<Graph.Transition>();

			foreach (var state in _graph.Graph.States)
			{
				foreach (var transition in state.ToTransitions)
				{
					if (transition.Label.IsTerminal) continue;
					if (transition.Label.IsInitial) continue;
					if (reachableTransitions.ContainsKey(transition)) continue;

					transitionsToDelete.Add(transition);
				}
			}

			foreach (var transition in transitionsToDelete)
			{
				_graph.Delete(transition);
			}
		}

		static Dictionary<Segment, Segment> CalculateTargetMappings(ParseItem[] items)
		{
			var mappings = new Dictionary<Segment, Segment>();
			var pending = new Queue<KeyValuePair<Segment, Segment>>();

			for (var i = 0; i < items.Length; i++)
			{
				var item = items[i];
				var production = item.Production;
				var token = production.Segments[0];

				if (!token.IsTerminal)
				{
					mappings.Add(token, production.Target);
					pending.Enqueue(new KeyValuePair<Segment, Segment>(token, production.Target));
				}
			}

			while (pending.Count > 0)
			{
				var pair = pending.Dequeue();

				if (mappings.TryGetValue(pair.Value, out var next))
				{
					mappings[pair.Key] = next;
					pending.Enqueue(new KeyValuePair<Segment, Segment>(pair.Key, next));
				}
			}

			return mappings;
		}

		ParseItem[] FindTrivialItems(ParseItemSet set)
		{
			var count = set.Count;
			var items = new ParseItem[count];
			var tokens = new Segment[count];
			var write = 0;

			set.CopyTo(items, 0);

			// Consider only incomplete items.
			for (var i = 0; i < count; i++)
			{
				var item = items[i];
				var production = item.Production;

				if (item.Position < production.Segments.Length)
				{
					items[write] = items[i];
					tokens[write] = production.Segments[item.Position];
					write++;
				}
			}

			if (write == 0)
			{
				return null;
			}

			count = write;
			write = 0;

			Array.Sort(tokens, items, 0, count);

			var duplicate = false;
			var prevToken = tokens[0];
			var prevItem = items[0];

			// Discard items that share the "next" segment or have a non-trivial action.
			for (var i = 1; i < count; i++)
			{
				var nextToken = tokens[i];
				var nextItem = items[i];

				if (nextToken == prevToken)
				{
					duplicate = true;
				}
				else
				{
					if (duplicate)
					{
						duplicate = false;
					}
					else if (!prevItem.Production.Target.IsInitial && _trivialLookup[prevItem.Production])
					{
						items[write] = prevItem;
						tokens[write] = prevToken;
						write++;
					}

					prevToken = nextToken;
					prevItem = nextItem;
				}
			}

			if (!duplicate && !prevItem.Production.Target.IsInitial && _trivialLookup[prevItem.Production])
			{
				items[write] = prevItem;
				tokens[write] = prevToken;
				write++;
			}

			if (write == 0)
			{
				return null;
			}

			count = write;

			if (count < items.Length)
			{
				Array.Resize(ref items, count);
			}

			return items;
		}

		static Graph.Transition GetTransition(Graph.State state, Segment segment)
		{
			foreach (var transition in state.ToTransitions)
			{
				if (transition.Label == segment)
				{
					return transition;
				}
			}

			return null;
		}

		static Dictionary<Production, bool> ExtractTrivialProductions(Config config)
		{
			var result = new Dictionary<Production, bool>();

			foreach (var production in config.Productions)
			{
				foreach (var rule in production.Rules)
				{
					if (result.ContainsKey(rule.Production))
					{
						result[rule.Production] = false;
					}
					else
					{
						result.Add(rule.Production, IsTrivial(rule));
					}
				}
			}

			return result;
		}

		Dictionary<Graph.Transition, bool> CalculatePopulateReachability()
		{
			var result = new Dictionary<Graph.Transition, bool>();
			var seen = new Dictionary<Graph.State, bool>();

			var queue = new Queue<Graph.State>();

			foreach (var state in _graph.Graph.StartStates)
			{
				queue.Enqueue(state);
			}

			while (queue.Count > 0)
			{
				var state = queue.Dequeue();

				if (seen.ContainsKey(state)) continue;
				seen.Add(state, true);

				foreach (var transition in state.ToTransitions)
				{
					if (transition.Label.IsTerminal)
					{
						result.Add(transition, true);
						queue.Enqueue(transition.ToState);
					}
				}

				foreach (var item in state.Label)
				{
					if (item.Position != item.Production.Segments.Length) continue;
					if (state.Label.GetLookahead(item).Count == 0) continue;
					if (item.Production.Target.IsInitial) continue;

					foreach (var backState in BackTrack(state, item.Position))
					{
						var transition = GetTransition(backState, item.Production.Target);

						if (transition != null && !result.ContainsKey(transition))
						{
							result.Add(transition, true);
							queue.Enqueue(transition.ToState);
						}
					}
				}
			}

			return result;
		}

		static IEnumerable<Graph.State> BackTrack(Graph.State state, int count)
		{
			if (count == 0)
			{
				yield return state;
				yield break;
			}

			var seen = new Dictionary<Graph.State, bool>();
			var queue = new Queue<KeyValuePair<Graph.State, int>>();
			queue.Enqueue(new KeyValuePair<Graph.State, int>(state, count));

			while (queue.Count > 0)
			{
				var pair = queue.Dequeue();

				if (pair.Value == 0)
				{
					if (!seen.ContainsKey(pair.Key))
					{
						seen.Add(pair.Key, true);
						yield return pair.Key;
					}

					continue;
				}

				foreach (var transition in pair.Key.FromTransitions)
				{
					queue.Enqueue(new KeyValuePair<Graph.State, int>(transition.FromState, pair.Value - 1));
				}
			}
		}

		static bool IsTrivial(ConfigRule rule)
		{
			if (rule.Segments.Count != 1) return false;

			return rule.Command is ConfigCommandArg;
		}

		readonly Config _config;
		readonly Graph.Builder _graph;
		readonly Dictionary<Production, bool> _trivialLookup;
	}
}
