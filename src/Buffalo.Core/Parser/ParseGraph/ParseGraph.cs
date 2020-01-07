// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Buffalo.Core.Parser.Configuration;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	sealed class ParseGraph : ICollection<KeyValuePair<Segment, Graph.State>>
	{
		ParseGraph(Graph graph, Segment[] topLevelSegments, Graph.State[] startStates)
		{
			Array.Sort(topLevelSegments, startStates);

			Graph = graph;
			_topLevelSegments = topLevelSegments;
			_startStates = startStates;
		}

		public static ParseGraph ConstructGraph(Config config)
		{
			var productions = ExtractProductions(config);
			var provider = new SegmentSetProvider(productions);
			return ConstructGraph(config, provider);
		}

		public int Count => _topLevelSegments.Length;
		public Graph Graph { get; }

		public Graph.State this[Segment segment]
		{
			get
			{
				for (var i = 0; i < _topLevelSegments.Length; i++)
				{
					if (_topLevelSegments[i].Equals(segment))
					{
						return _startStates[i];
					}
				}

				return null;
			}
		}

		void ICollection<KeyValuePair<Segment, Graph.State>>.Add(KeyValuePair<Segment, Graph.State> item)
			=> throw new NotSupportedException();

		void ICollection<KeyValuePair<Segment, Graph.State>>.Clear()
			=> throw new NotSupportedException();

		bool ICollection<KeyValuePair<Segment, Graph.State>>.Contains(KeyValuePair<Segment, Graph.State> item)
		{
			var index = Array.BinarySearch(_topLevelSegments, item.Key);
			return index >= 0 && _startStates[index] == item.Value;
		}

		void ICollection<KeyValuePair<Segment, Graph.State>>.CopyTo(KeyValuePair<Segment, Graph.State>[] array, int arrayIndex)
		{
			for (var i = 0; i < _topLevelSegments.Length; i++)
			{
				array[i + arrayIndex] = new KeyValuePair<Segment, Graph.State>(_topLevelSegments[i], _startStates[i]);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection<KeyValuePair<Segment, Graph.State>>.IsReadOnly => true;

		bool ICollection<KeyValuePair<Segment, Graph.State>>.Remove(KeyValuePair<Segment, Graph.State> item)
			=> throw new NotSupportedException();

		public IEnumerator<KeyValuePair<Segment, Graph.State>> GetEnumerator()
		{
			for (var i = 0; i < _topLevelSegments.Length; i++)
			{
				yield return new KeyValuePair<Segment, Graph.State>(_topLevelSegments[i], _startStates[i]);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		static void EnqueueRange<T>(Queue<T> queue, IEnumerable<T> range)
		{
			foreach (var item in range)
			{
				queue.Enqueue(item);
			}
		}

		static Dictionary<Segment, Production[]> ExtractProductions(Config config)
		{
			var productions = new Dictionary<Segment, List<Production>>();

			foreach (var nonTerminal in config.Productions)
			{
				var list = new List<Production>();

				if (!productions.TryGetValue(nonTerminal.Segment, out list))
				{
					list = new List<Production>();
					productions.Add(nonTerminal.Segment, list);
				}

				foreach (var rule in nonTerminal.Rules)
				{
					list.Add(rule.Production);
				}
			}

			var result = new Dictionary<Segment, Production[]>();

			foreach (var pair in productions)
			{
				result.Add(pair.Key, pair.Value.ToArray());
			}

			foreach (var topLevelSegment in config.TopLevelSegments)
			{
				var initial = topLevelSegment.GetInitial();
				var startProduction = new Production(initial, ImmutableArray.Create(topLevelSegment));
				result.Add(initial, new Production[] { startProduction });
			}

			return result;
		}

		static ParseGraph ConstructGraph(Config config, SegmentSetProvider provider)
		{
			var graphBuilder = new Graph.Builder();
			var map = new Dictionary<ParseItemSet, Graph.State>();
			var queue = new Queue<Graph.State>();

			var startStates = new List<Graph.State>();
			var topLevelSegments = new List<Segment>();

			foreach (var segment in provider.InitialSegments)
			{
				var initialKernel = ParseItemSet.New(provider.CreateParseItems(segment));
				var initialState = graphBuilder.NewState(true, Closure(provider, initialKernel));
				map.Add(initialKernel, initialState);
				queue.Enqueue(initialState);
				startStates.Add(initialState);
				topLevelSegments.Add(new Segment(segment.Name, false));
			}

			while (queue.Count > 0)
			{
				var fromState = queue.Dequeue();

				foreach (var pair in fromState.Label.GetTransitionKernels())
				{
					if (!map.TryGetValue(pair.Value, out var toState))
					{
						toState = graphBuilder.NewState(false, Closure(provider, pair.Value));
						map.Add(pair.Value, toState);
						queue.Enqueue(toState);
					}

					graphBuilder.AddTransition(fromState, toState, pair.Key);
				}
			}

			var graph = graphBuilder.Graph;
			PopulateLookaheads(provider, graph);
			ParseGraphOptimiser.Optimise(config, graphBuilder);

			return new ParseGraph(graph, topLevelSegments.ToArray(), startStates.ToArray());
		}

		static ParseItemSet Closure(SegmentSetProvider provider, ParseItemSet kernelItems)
		{
			var lookup = new Dictionary<ParseItem, bool>();
			var itemsToAdd = new Queue<ParseItem>();

			EnqueueRange(itemsToAdd, kernelItems);

			while (itemsToAdd.Count > 0)
			{
				var item = itemsToAdd.Dequeue();
				var production = item.Production;

				if (!lookup.ContainsKey(item))
				{
					lookup.Add(item, true);

					if (item.Position < production.Segments.Length)
					{
						var next = production.Segments[item.Position];

						if (!next.IsTerminal)
						{
							EnqueueRange(itemsToAdd, provider.CreateParseItems(next));
						}
					}
				}
			}

			return ParseItemSet.New(lookup.Keys);
		}

		static void PopulateLookaheads(SegmentSetProvider provider, Graph graph)
		{
			var pending = new Queue<KeyValuePair<Graph.State, ParseItem>>();
			var eofSet = SegmentSet.New(new Segment[] { Segment.EOF });

			foreach (var state in graph.StartStates)
			{
				foreach (var item in state.Label)
				{
					if (item.Production.Target.IsInitial)
					{
						state.Label.SetLookahead(item, eofSet);
						pending.Enqueue(new KeyValuePair<Graph.State, ParseItem>(state, item));
					}
				}
			}

			while (pending.Count > 0)
			{
				var pair = pending.Dequeue();
				var thisState = pair.Key;
				var thisItem = pair.Value;

				var thisLookahead = thisState.Label.GetLookahead(thisItem);
				var next = thisItem.Production.Segments[thisItem.Position];

				foreach (var transition in thisState.ToTransitions)
				{
					if (transition.Label != next) continue;

					var nextItem = thisItem.NextItem();

					if (transition.ToState.Label.TryUnionLookahead(nextItem, thisLookahead) &&
						nextItem.Position != nextItem.Production.Segments.Length)
					{
						pending.Enqueue(new KeyValuePair<Graph.State, ParseItem>(transition.ToState, nextItem));
					}

					break;
				}

				if (!next.IsTerminal)
				{
					var follow = provider.GetFollowSets(thisItem.Production, thisItem.Position);

					if (follow.ContainsSegment(null))
					{
						follow = thisLookahead.Union(follow.Subtract(SegmentSet.EpsilonSet));
					}

					foreach (var nextItem in thisState.Label)
					{
						if (nextItem.Position != 0) continue;
						if (nextItem.Production.Target != next) continue;

						if (thisState.Label.TryUnionLookahead(nextItem, follow) &&
							nextItem.Position != nextItem.Production.Segments.Length)
						{
							pending.Enqueue(new KeyValuePair<Graph.State, ParseItem>(thisState, nextItem));
						}
					}
				}
			}
		}

		readonly Segment[] _topLevelSegments;
		readonly Graph.State[] _startStates;
	}
}
