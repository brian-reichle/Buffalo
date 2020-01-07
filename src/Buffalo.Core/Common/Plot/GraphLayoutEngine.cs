// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Buffalo.Core.Common
{
	sealed class GraphLayoutEngine<TNode, TTransition>
	{
		public static void Calculate(IGraphLayoutSpec<TNode, TTransition> spec, IGraphLayoutInfo<TNode, TTransition> info, Graph<TNode, TTransition> graph)
		{
			if (graph.StartStates.Count > 0)
			{
				new GraphLayoutEngine<TNode, TTransition>(spec, info, graph).CalculateCore();
			}
		}

		GraphLayoutEngine(IGraphLayoutSpec<TNode, TTransition> spec, IGraphLayoutInfo<TNode, TTransition> info, Graph<TNode, TTransition> graph)
		{
			_spec = spec;
			_info = info;
			_builder = CreateGraph(graph);
			_graph = _builder.Graph;
		}

		static Graph<StatePlotToken, TransitionPlotToken>.Builder CreateGraph(Graph<TNode, TTransition> graph)
		{
			var result = new Graph<StatePlotToken, TransitionPlotToken>.Builder();
			var lookup = new Dictionary<Graph<TNode, TTransition>.State, Graph<StatePlotToken, TransitionPlotToken>.State>();

			foreach (var state in graph.States)
			{
				lookup.Add(state, result.NewState(state.IsStartState, new StatePlotToken()
				{
					State = state,
				}));
			}

			foreach (var fromState in result.Graph.States)
			{
				foreach (var transition in fromState.Label.State.ToTransitions)
				{
					var toState = lookup[transition.ToState];

					result.AddTransition(fromState, toState, new TransitionPlotToken()
					{
						Transition = transition,
					});
				}
			}

			return result;
		}

		void CalculateCore()
		{
			CalculateRanks();
			InsertInterTransitionalStates();
			CalculateOrderings();
			InsertIntraTransitionalStates();
			CalculatePositions();
			CalculateSplines();
		}

		void CalculateRanks()
		{
			var queue = new Queue<Graph<StatePlotToken, TransitionPlotToken>.State>();

			foreach (var state in _graph.States)
			{
				if (state.IsStartState || state.FromTransitions.Count == 0)
				{
					state.Label.Rank = 1;
					queue.Enqueue(state);
				}
			}

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				var rank = current.Label.Rank;

				foreach (var transition in current.ToTransitions)
				{
					var next = transition.ToState;
					var nextLabel = next.Label;

					if (nextLabel.Rank == 0)
					{
						nextLabel.Rank = rank + 2;
						queue.Enqueue(next);
					}
				}
			}
		}

		void InsertInterTransitionalStates()
		{
			var transitions = new List<Graph<StatePlotToken, TransitionPlotToken>.Transition>();

			foreach (var state in _graph.States)
			{
				var fromRank = state.Label.Rank;

				foreach (var transition in state.ToTransitions)
				{
					if (transition.ToState.Label.Rank != fromRank)
					{
						transitions.Add(transition);
					}
				}
			}

			foreach (var transition in transitions)
			{
				var fromState = transition.FromState;
				var toState = transition.ToState;
				var label = transition.Label;

				var rank1 = fromState.Label.Rank;
				var rank2 = toState.Label.Rank;
				var diff = rank2 - rank1;
				int step;

				if (diff < -1)
				{
					step = -1;
				}
				else if (diff > 1)
				{
					step = 1;
				}
				else
				{
					throw new InvalidOperationException("WTF?!?!");
				}

				_builder.Delete(transition);

				var prevState = fromState;

				for (var rank = rank1 + step; rank != rank2; rank += step)
				{
					var nextState = _builder.NewState(false, new StatePlotToken()
					{
						Rank = rank,
					});

					if (label.LabelToken == null)
					{
						var token = nextState.Label;
						token.Height = _spec.LabelHeight(label.Transition);
						token.Width = _spec.LabelWidth(label.Transition);

						label.LabelToken = token;
					}

					_builder.AddTransition(prevState, nextState, label);

					prevState = nextState;
				}

				_builder.AddTransition(prevState, toState, label);
			}
		}

		void CalculateOrderings()
		{
			SetupStateGrid();

			Converter<Graph<StatePlotToken, TransitionPlotToken>.State, int> forWeightCalculator = WeightFor;
			Converter<Graph<StatePlotToken, TransitionPlotToken>.State, int> revWeightCalculator = WeightRev;

			for (var n = 0; n < 25; n++)
			{
				for (var i = 1; i < _stateGrid.Length; i++)
				{
					ReOrder(_stateGrid[i], revWeightCalculator);
				}

				for (var i = _stateGrid.Length - 2; i >= 0; i--)
				{
					ReOrder(_stateGrid[i], forWeightCalculator);
				}

				Transpose();
			}
		}

		void InsertIntraTransitionalStates()
		{
			var loopTransitions = new List<Graph<StatePlotToken, TransitionPlotToken>.Transition>();

			foreach (var state in _graph.States)
			{
				var fromRank = state.Label.Rank;

				foreach (var transition in state.ToTransitions)
				{
					if (transition.ToState.Label.Rank == fromRank)
					{
						loopTransitions.Add(transition);
					}
				}
			}

			foreach (var transition in loopTransitions)
			{
				var label = transition.Label;
				var fromState = transition.FromState;
				var toState = transition.ToState;
				var midState = _builder.NewState(false, new StatePlotToken()
				{
					Rank = fromState.Label.Rank,
					Ordering = (fromState.Label.Ordering + toState.Label.Ordering) >> 1,
					Width = _spec.LabelWidth(label.Transition),
					Height = _spec.LabelHeight(label.Transition),
				});

				label.LabelToken = midState.Label;

				_builder.Delete(transition);

				_builder.AddTransition(fromState, midState, label);
				_builder.AddTransition(midState, toState, label);
			}

			SetupStateGrid();
		}

		void CalculatePositions()
		{
			CalculateInitialOrderPositioning();

			ShortenEdges();

			CalculateInitialRankPositioning();

			foreach (var state in _graph.States)
			{
				if (state.Label.State != null)
				{
					Report(state.Label);
				}
			}
		}

		void CalculateInitialRankPositioning()
		{
			var current = 0;

			for (var r = 0; r < _stateGrid.Length; r++)
			{
				var row = _stateGrid[r];
				var delta = 0;
				var maxDisplacement = 0;

				for (var i = 0; i < row.Length; i++)
				{
					var token = row[i].Label;

					var width = token.State == null ? token.Width : _spec.NodeWidth(token.State);
					if (width > delta) delta = width;

					token.Width = width;
					maxDisplacement = Math.Max(maxDisplacement, CalculateMaxDisplacement(row[i]));
				}

				for (var i = 0; i < row.Length; i++)
				{
					row[i].Label.X = current + (delta >> 1);
				}

				current += delta + Math.Max((int)Math.Ceiling(maxDisplacement * 0.58), _spec.RankSeparation);
			}
		}

		void CalculateInitialOrderPositioning()
		{
			var current = 0;
			var order = 0;
			bool cont;

			do
			{
				cont = false;
				var delta = 0;

				for (var r = 0; r < _stateGrid.Length; r++)
				{
					var row = _stateGrid[r];

					if (order < row.Length)
					{
						var token = row[order].Label;

						var height = token.State == null ? token.Height : _spec.NodeHeight(token.State);
						if (height > delta) delta = height;

						token.Height = height;
						cont = true;
					}
				}

				if (!cont) break;

				for (var r = 0; r < _stateGrid.Length; r++)
				{
					var row = _stateGrid[r];

					if (order < row.Length)
					{
						row[order].Label.Y = current + (delta >> 1);
					}
				}

				current += delta + _spec.NodeSeparation;
				order++;
			}
			while (true);
		}

		void ShortenEdges()
		{
			var queue = new Queue<Graph<StatePlotToken, TransitionPlotToken>.State>();

			for (var i = 0; i < _stateGrid.Length; i++)
			{
				var row = _stateGrid[i];
				queue.Enqueue(row[row.Length - 1]);
			}

			while (queue.Count > 0)
			{
				var state = queue.Dequeue();
				var upperState = GetNextStateOnRank(state, -1);
				var lowerState = GetNextStateOnRank(state, 1);
				var desired = CalculateDesiredPos(state);
				int newPos;

				if (desired < state.Label.Y)
				{
					if (upperState == null)
					{
						newPos = desired;
					}
					else
					{
						var nearest = upperState.Label.Y + ((upperState.Label.Height + state.Label.Height) >> 1) + _spec.NodeSeparation;
						newPos = Math.Max(nearest, desired);
					}
				}
				else if (desired > state.Label.Y)
				{
					if (lowerState == null)
					{
						newPos = desired;
					}
					else
					{
						var nearest = lowerState.Label.Y - ((lowerState.Label.Height + state.Label.Height) >> 1) - _spec.NodeSeparation;
						newPos = Math.Min(nearest, desired);
					}
				}
				else
				{
					continue;
				}

				if (newPos != state.Label.Y)
				{
					state.Label.Y = newPos;

					if (upperState != null) queue.Enqueue(upperState);
					if (lowerState != null) queue.Enqueue(lowerState);

					foreach (var transition in state.ToTransitions)
					{
						queue.Enqueue(transition.ToState);
					}

					foreach (var transition in state.FromTransitions)
					{
						queue.Enqueue(transition.FromState);
					}
				}
			}
		}

		static int CalculateDesiredPos(Graph<StatePlotToken, TransitionPlotToken>.State state)
		{
			var total = 0;
			var count = 0;

			int lightWeight;
			int heavyWeight;

			if (state.Label.State == null)
			{
				lightWeight = 2;
				heavyWeight = 8;
			}
			else
			{
				lightWeight = 1;
				heavyWeight = 2;
			}

			foreach (var transition in state.ToTransitions)
			{
				if (transition.ToState == state) continue;
				var other = transition.ToState.Label;
				var weight = other.State == null ? heavyWeight : lightWeight;

				total += weight * other.Y;
				count += weight;
			}

			foreach (var transition in state.FromTransitions)
			{
				if (transition.FromState == state) continue;
				var other = transition.FromState.Label;
				var weight = other.State == null ? heavyWeight : lightWeight;

				total += weight * other.Y;
				count += weight;
			}

			if (count == 0)
			{
				return state.Label.Y;
			}
			else
			{
				return total / count;
			}
		}

		static int CalculateMaxDisplacement(Graph<StatePlotToken, TransitionPlotToken>.State state)
		{
			var result = 0;
			var y = state.Label.Y;
			var targetRank = state.Label.Rank + 1;

			foreach (var transition in state.ToTransitions)
			{
				var otherStateLabel = transition.ToState.Label;

				if (otherStateLabel.Rank == targetRank)
				{
					var diff = Math.Abs(otherStateLabel.Y - y);

					if (diff > result)
					{
						result = diff;
					}
				}
			}

			foreach (var transition in state.FromTransitions)
			{
				var otherStateLabel = transition.FromState.Label;

				if (otherStateLabel.Rank == targetRank)
				{
					var diff = Math.Abs(otherStateLabel.Y - y);

					if (diff > result)
					{
						result = diff;
					}
				}
			}

			return result;
		}

		void CalculateSplines()
		{
			var states = new List<StatePlotToken>();

			foreach (var state in _graph.States)
			{
				if (state.Label.State == null) continue;

				foreach (var transition in state.ToTransitions)
				{
					states.Add(transition.FromState.Label);

					var current = transition;

					do
					{
						var nextState = current.ToState;

						states.Add(nextState.Label);

						if (nextState.Label.State == null)
						{
							current = nextState.ToTransitions[0];
						}
						else
						{
							break;
						}
					}
					while (true);

					Report(transition.Label, states);

					states.Clear();
				}
			}
		}

		void Report(StatePlotToken token)
		{
			_info.SetStatePosition(
				token.State,
				token.X - (token.Width >> 1),
				token.Y - (token.Height >> 1),
				token.Width,
				token.Height);
		}

		void Report(TransitionPlotToken transition, List<StatePlotToken> states)
		{
			var rLen = (states.Count * 3) - 2;

			var xs = new int[rLen];
			var ys = new int[rLen];

			var prevX = states[0].X;
			var prevY = states[0].Y;
			var prevRank = states[0].Rank;
			var prevOrderDecreasing = true;

			xs[0] = prevX;
			ys[0] = prevY;

			var n = 1;

			for (var i = 1; i < states.Count; i++)
			{
				var x = states[i].X;
				var y = states[i].Y;
				var rank = states[i].Rank;
				int midX;

				if (rank != prevRank)
				{
					midX = (prevX + x) >> 1;
				}
				else
				{
					var orderDecreasing = states[i - 1].Ordering > states[i].Ordering;
					var onRight = (i != 1 && orderDecreasing == prevOrderDecreasing) == orderDecreasing;

					var xofs = (states[i].Width + states[i - 1].Width + _spec.RankSeparation + _spec.RankSeparation) >> 2;
					xofs = Math.Max((int)Math.Ceiling(Math.Abs(prevY - y) * 0.58), xofs);

					midX = x + (onRight ? xofs : -xofs);

					prevOrderDecreasing = orderDecreasing;
				}

				xs[n] = (x + midX) >> 1;
				ys[n] = prevY;

				xs[n + 1] = (prevX + midX) >> 1;
				ys[n + 1] = y;

				xs[n + 2] = x;
				ys[n + 2] = y;

				prevX = x;
				prevY = y;
				prevRank = rank;
				n += 3;
			}

			if (transition.LabelToken != null)
			{
				var label = transition.LabelToken;
				_info.SetTransitionLabel(transition.Transition, label.X - (label.Width >> 1), label.Y - (label.Height >> 1), label.Width, label.Height);
			}

			_info.SetTransitionPath(transition.Transition, xs, ys);
		}

		static void ReOrder(Graph<StatePlotToken, TransitionPlotToken>.State[] states, Converter<Graph<StatePlotToken, TransitionPlotToken>.State, int> weightCalculator)
		{
			var weights = Array.ConvertAll(states, weightCalculator);
			Array.Sort(weights, states);

			for (var i = 0; i < states.Length; i++)
			{
				states[i].Label.Ordering = i + 1;
			}
		}

		void Transpose()
		{
			const int InitialCount = 10;
			bool improved;
			bool fiddled;
			var countDown = InitialCount;

			do
			{
				improved = false;
				fiddled = false;

				for (var r = 0; r < _stateGrid.Length; r++)
				{
					var dir = Transpose(r);

					if (dir > 0)
					{
						improved = true;
					}
					else if (dir == 0)
					{
						fiddled = true;
					}
				}

				if (improved)
				{
					countDown = InitialCount;
				}
				else if (fiddled && countDown > 0)
				{
					countDown--;
				}
				else
				{
					break;
				}
			}
			while (true);
		}

		int Transpose(int rankToUpdate)
		{
			var rank = _stateGrid[rankToUpdate];

			var improved = false;
			var fiddled = false;

			for (var i = 1; i < rank.Length; i++)
			{
				var s1 = rank[i - 1];
				var s2 = rank[i];

				var diff = CrossingDiff(s1, s2);

				if (diff >= 0)
				{
					s1.Label.Ordering = i + 1;
					s2.Label.Ordering = i;

					rank[i - 1] = s2;
					rank[i] = s1;

					fiddled = true;

					if (diff > 0)
					{
						improved = true;
					}
				}
			}

			if (improved)
			{
				return 1;
			}
			else if (fiddled)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}

		static int CrossingDiff(Graph<StatePlotToken, TransitionPlotToken>.State state1, Graph<StatePlotToken, TransitionPlotToken>.State state2)
		{
			var rank = state1.Label.Rank;
			var crossings1 = 0;
			var crossings2 = 0;

			CountCrossings(state1, state2, rank - 1, ref crossings1, ref crossings2);
			CountCrossings(state1, state2, rank + 1, ref crossings1, ref crossings2);

			return crossings1 - crossings2;
		}

		static void CountCrossings(Graph<StatePlotToken, TransitionPlotToken>.State state1, Graph<StatePlotToken, TransitionPlotToken>.State state2, int otherRank, ref int crossings1, ref int crossings2)
		{
			var orderList1 = GetOrders(state1, otherRank);
			var orderList2 = GetOrders(state2, otherRank);

			if (orderList1.Count > 0 && orderList2.Count > 0)
			{
				orderList1.Sort();
				orderList2.Sort();

				var count1 = 0;
				var count2 = 0;

				var i1 = 0;
				var i2 = 0;

				while (i1 < orderList1.Count && i2 < orderList2.Count)
				{
					var v1 = orderList1[i1];
					var v2 = orderList2[i2];

					var diff = v1 - v2;

					if (diff < 0)
					{
						crossings1 += count2;
						count1++;
						i1++;
					}
					else if (diff > 0)
					{
						crossings2 += count1;
						count2++;
						i2++;
					}
					else
					{
						crossings1 += count2;
						crossings2 += count1;
						count1++;
						count2++;
						i1++;
						i2++;
					}
				}

				if (i1 < orderList1.Count)
				{
					crossings1 += count2 * (orderList1.Count - i1);
				}

				if (i2 < orderList2.Count)
				{
					crossings2 += count1 * (orderList2.Count - i2);
				}
			}
		}

		int WeightFor(Graph<StatePlotToken, TransitionPlotToken>.State state)
			=> Weight(state.Label.Ordering, GetOrders(state, state.Label.Rank + 1));

		int WeightRev(Graph<StatePlotToken, TransitionPlotToken>.State state)
			=> Weight(state.Label.Ordering, GetOrders(state, state.Label.Rank - 1));

		static int Weight(int existing, List<int> weights)
		{
			RemoveDuplicates(weights);

			switch (weights.Count)
			{
				case 0: return existing << 1;
				case 1: return weights[0] << 1;
				case 2: return weights[0] + weights[1];
				default:
					var mid = weights.Count >> 1;

					if ((weights.Count & 1) == 1)
					{
						return weights[mid] << 1;
					}
					else
					{
						return weights[mid] + weights[mid - 1];
					}
			}
		}

		void SetupStateGrid()
		{
			var stateGrid = new List<Graph<StatePlotToken, TransitionPlotToken>.State[]>();
			var stateList = new List<Graph<StatePlotToken, TransitionPlotToken>.State>();

			var allStates = _graph.States.ToArray();
			Array.Sort(allStates, (s1, s2) =>
			{
				var diff = s1.Label.Rank - s2.Label.Rank;
				if (diff != 0) return diff;

				return s1.Label.Ordering - s2.Label.Ordering;
			});

			var lastRank = 1;

			for (var i = 0; i < allStates.Length; i++)
			{
				var token = allStates[i].Label;

				if (token.Rank != lastRank)
				{
					stateGrid.Add(stateList.ToArray());
					stateList.Clear();
					lastRank = token.Rank;
				}

				stateList.Add(allStates[i]);
				token.Ordering = stateList.Count;
			}

			stateGrid.Add(stateList.ToArray());
			stateList.Clear();

			_stateGrid = stateGrid.ToArray();
		}

		Graph<StatePlotToken, TransitionPlotToken>.State GetNextStateOnRank(Graph<StatePlotToken, TransitionPlotToken>.State state, int offset)
		{
			var label = state.Label;
			var r = label.Rank - 1;
			var o = label.Ordering - 1 + offset;

			var row = _stateGrid[r];
			return (o >= 0 && o < row.Length) ? row[o] : null;
		}

		static List<int> GetOrders(Graph<StatePlotToken, TransitionPlotToken>.State state, int rank)
		{
			var orders = new List<int>();

			foreach (var transition in state.FromTransitions)
			{
				var fromLabel = transition.FromState.Label;

				if (fromLabel.Rank != rank) continue;
				orders.Add(fromLabel.Ordering);
			}

			foreach (var transition in state.ToTransitions)
			{
				var toLabel = transition.ToState.Label;

				if (toLabel.Rank != rank) continue;
				orders.Add(toLabel.Ordering);
			}

			return orders;
		}

		static void RemoveDuplicates(List<int> values)
		{
			if (values.Count > 1)
			{
				values.Sort();

				var write = 1;
				for (var read = 1; read < values.Count; read++)
				{
					if (values[read] != values[read - 1])
					{
						if (write != read)
						{
							values[write] = values[read];
						}

						write++;
					}
				}

				if (write < values.Count)
				{
					values.RemoveRange(write, values.Count - write);
				}
			}
		}

		Graph<StatePlotToken, TransitionPlotToken>.State[][] _stateGrid;

		readonly Graph<StatePlotToken, TransitionPlotToken> _graph;
		readonly Graph<StatePlotToken, TransitionPlotToken>.Builder _builder;
		readonly IGraphLayoutSpec<TNode, TTransition> _spec;
		readonly IGraphLayoutInfo<TNode, TTransition> _info;

		#region Tokens

		[DebuggerDisplay("{State.StateId}: ({Rank}, {Ordering}) ({X}, {Y}, {Width}, {Height})")]
		sealed class StatePlotToken
		{
			public int Rank { get; set; }
			public int Ordering { get; set; }

			public int X { get; set; }
			public int Y { get; set; }

			public int Width { get; set; }
			public int Height { get; set; }

			public Graph<TNode, TTransition>.State State { get; set; }
		}

		sealed class TransitionPlotToken
		{
			public Graph<TNode, TTransition>.Transition Transition { get; set; }
			public StatePlotToken LabelToken { get; set; }
		}

		#endregion
	}
}
