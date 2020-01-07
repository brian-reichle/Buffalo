// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using Buffalo.Core.Common;
using Buffalo.Core.Parser.Configuration;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	sealed class TableGenerator
	{
		public static TableData GenerateTables(Config config)
		{
			var graph = config.Graph.Graph;
			var gotoMap = GetGotoMappings(graph);
			var terminalMap = GetTerminalMappings(graph, out var terminalMask, out var terminalColumns);
			var stateMap = GetStateMappings(graph);
			var reductionMap = GetReductionMappings(graph);

			var statistics = new Statistics()
			{
				Reductions = reductionMap.Count,
				Terminals = terminalMap.Count,
				NonTerminals = gotoMap.Count,
				States = stateMap.Count,
				TerminalColumns = terminalColumns,
			};

			var result = new TableData()
			{
				Statistics = statistics,
				GotoMap = gotoMap,
				TerminalMap = terminalMap,
				StateMap = stateMap,
				ReductionMap = reductionMap,
				TerminalMask = terminalMask,
				TerminalColumns = terminalColumns,
				NeedsTerminalMask = terminalColumns != terminalMap.Count,
			};

			result.Actions = ExtractTransitionTableBlob(config, statistics, result);

			return result;
		}

		static CompressedBlob ExtractTransitionTableBlob(Config config, Statistics statistics, TableData data)
		{
			var graph = config.Graph.Graph;
			var actionsList = new int[data.StateMap.Count];
			var lastGotoStateIndex = data.StateMap.Count;

			var fragments = new List<TableFragment>();

			foreach (var state in graph.States)
			{
				var index = data.StateMap[state];
				var transitionRow = ExtractTransitionRow(data, state);
				var gotoRow = ExtractGotoRow(data, state);

				if (transitionRow.Fragment == null)
				{
					actionsList[index] = transitionRow.ShortCircuitReduction;
					statistics.StatesShortCircuited++;

					if (gotoRow != null)
					{
						statistics.StatesWithGotos++;
					}
				}
				else
				{
					fragments.Add(transitionRow.Fragment);

					if (transitionRow.HasReduction && transitionRow.HasShift)
					{
						statistics.StatesWithSRConflicts++;
					}

					if (gotoRow != null)
					{
						statistics.StatesWithGotos++;
					}
					else if (!transitionRow.HasReduction || !transitionRow.HasShift)
					{
						statistics.StatesOther++;
					}
				}

				if (gotoRow != null)
				{
					fragments.Add(gotoRow);
					statistics.NonTerminalColumns = Math.Max(statistics.NonTerminalColumns, gotoRow.Count);
					lastGotoStateIndex = data.StateMap[state] + 1;
				}
			}

			var combined = TableFragment.Combine(fragments);

			var offsetSectionLen = graph.States.Count + lastGotoStateIndex;
			Array.Resize(ref actionsList, offsetSectionLen + combined.Count);

			for (var i = 0; i < graph.States.Count << 1; i++)
			{
				var offset = combined.GetOffset(i);

				if (offset.HasValue)
				{
					actionsList[i] = offset.Value + offsetSectionLen;
				}
			}

			combined.CopyTo(actionsList, offsetSectionLen);

			var actionsBlob = CompressedBlob.Compress(config.Manager.TableCompression, ElementSizeStrategy.Get(config.Manager.ElementSize), actionsList);

			statistics.ActionsRunTime = actionsList.Length;
			statistics.ActionsAssemblyBytes = actionsBlob.Bytes;
			statistics.GotoOffsetsLen = lastGotoStateIndex;
			return actionsBlob;
		}

		/// <summary>
		/// Generate a dictionary that maps states to table offset indices.
		/// </summary>
		/// <remarks>
		/// By placing states with goto's before states without goto's, we can reduce the amount of
		/// space in the transition table we need to reserve for the goto offset lookup. (states
		/// without goto's will never have their goto offsets queried and so that portion of the
		/// lookup can be omitted.
		/// </remarks>
		static Dictionary<Graph.State, int> GetStateMappings(Graph graph)
		{
			var result = new Dictionary<Graph.State, int>();
			var statesWithoutGotos = new List<Graph.State>(result.Count);
			var next = 0;

			foreach (var state in graph.States)
			{
				var hasGoto = false;

				foreach (var transition in state.ToTransitions)
				{
					if (!transition.Label.IsTerminal)
					{
						hasGoto = true;
						break;
					}
				}

				if (hasGoto)
				{
					result[state] = next++;
				}
				else
				{
					statesWithoutGotos.Add(state);
				}
			}

			foreach (var state in statesWithoutGotos)
			{
				result[state] = next++;
			}

			return result;
		}

		/// <summary>
		/// Generate a dictionary that maps non-terminals to goto column indices.
		/// </summary>
		/// <remarks>
		/// If 2 non-terminals never have goto's for the same states then they can safely occupy the
		/// same "column" in the transition table. This method also sorts the columns so that the more
		/// frequently used columns have a lower index, making it easier to overlap with other rows.
		/// </remarks>
		static Dictionary<Segment, int> GetGotoMappings(Graph graph)
		{
			var list = NonTerminalGrouping.GroupSegments(graph);
			var result = new Dictionary<Segment, int>();

			for (var i = 0; i < list.Length; i++)
			{
				foreach (var nonTerminal in list[i])
				{
					result[nonTerminal] = i;
				}
			}

			return result;
		}

		/// <summary>
		/// Generate a dictionary that maps terminals to action column indices.
		/// </summary>
		/// <remarks>
		/// This method attemps to place the more frequently used terminals in the middle of the rows,
		/// this increases the number of row overlapping by increasing the number of values at the end
		/// of the rows that are likely to overlap with other rows (the error value, '0', is by far
		/// the most likely to overlap with other rows.
		/// </remarks>
		static Dictionary<Segment, int> GetTerminalMappings(Graph graph, out int mask, out int columns)
		{
			var groups = TerminalGrouping.GroupSegments(graph);

			columns = groups.Length;
			mask = BitOperations.CopyBitsDown(columns);

			var unitCount = mask + 1;

			var result = new Dictionary<Segment, int>();

			for (var i = 0; i < groups.Length; i++)
			{
				var column = (i & 1) == 1 ? (i >> 1) : groups.Length - (i >> 1) - 1;

				var segments = groups[i];

				for (var n = 0; n < segments.Length; n++)
				{
					result[segments[n]] = column + (unitCount * n);
				}
			}

			return result;
		}

		static Dictionary<Production, int> GetReductionMappings(Graph graph)
		{
			var result = new Dictionary<Production, int>();

			foreach (var state in graph.States)
			{
				foreach (var item in state.Label)
				{
					var production = item.Production;
					if (item.Position != production.Segments.Length) continue;
					if (production.Target.IsInitial) continue;
					if (result.ContainsKey(production)) continue;
					if (state.Label.GetLookahead(item).Count == 0) continue;

					result.Add(production, result.Count);
				}
			}

			return result;
		}

		static ParseTableRow ExtractTransitionRow(TableData data, Graph.State state)
		{
			var actions = new int[data.TerminalColumns];
			var reduction = 0;
			var containsReduction = false;
			var containsShift = false;
			var reductionCount = data.ReductionMap.Count + 1;

			var mergeMask = ~data.TerminalMask;

			foreach (var item in state.Label)
			{
				if (item.Position != item.Production.Segments.Length) continue;

				var set = state.Label.GetLookahead(item);
				if (set.Count == 0) continue;

				int actionId;
				var production = item.Production;

				if (production.Target.IsInitial)
				{
					actionId = 1;
				}
				else
				{
					actionId = data.ReductionMap[production] + 2;
				}

				if (containsReduction)
				{
					reduction = -1;
				}
				else
				{
					reduction = actionId;
					containsReduction = true;
				}

				foreach (var la in set)
				{
					var index = data.TerminalMap[la];

					if ((index & mergeMask) == 0)
					{
						actions[index] = actionId;
					}
				}
			}

			foreach (var transition in state.ToTransitions)
			{
				if (!transition.Label.IsTerminal) continue;
				var index = data.TerminalMap[transition.Label];

				if ((index & mergeMask) == 0)
				{
					containsShift = true;
					reduction = -1;

					var actionId = data.StateMap[transition.ToState] + reductionCount;

					actions[index] = actionId;
				}
			}

			if (reduction > 0)
			{
				return new ParseTableRow(reduction);
			}
			else
			{
				FillInActionGaps(actions);
				return new ParseTableRow(new TableFragment(actions, data.StateMap[state]), containsReduction, containsShift);
			}
		}

		static TableFragment ExtractGotoRow(TableData data, Graph.State state)
		{
			var actions = new List<int>();
			var containsGoto = false;

			var gotoList = new Graph.State[data.GotoMap.Count];

			foreach (var transition in state.ToTransitions)
			{
				if (transition.Label.IsTerminal) continue;

				var index = data.GotoMap[transition.Label];

				containsGoto = true;
				gotoList[index] = transition.ToState;
			}

			if (containsGoto)
			{
				var nextActionIndex = 0;
				var firstGotoValue = -1;

				for (var i = 0; i < gotoList.Length; i++)
				{
					var x = gotoList[i];

					if (x != null)
					{
						var actionId = data.StateMap[x];

						if (firstGotoValue == -1)
						{
							firstGotoValue = i;
							nextActionIndex = i;
						}
						else
						{
							for (; nextActionIndex < i; nextActionIndex++)
							{
								actions.Add(0);
							}
						}

						actions.Add(actionId);
						nextActionIndex++;
					}
				}

				return new TableFragment(actions.ToArray(), data.StateMap[state] + data.StateMap.Count, firstGotoValue);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Attempt to improve the compress-ability of the table by filling in the gaps in the action rows.
		/// </summary>
		/// <remarks>
		/// The mechanisim used to compress the table for storage in the assembly looks for sequences of
		/// identical values. The effectiveness of this can be improved by copying reduction actions to
		/// neighbouring columns that would otherwise contain the error action. Error actions at the ends
		/// of the rows will not be changed as that could adversly affect row overlapping.
		/// </remarks>
		static void FillInActionGaps(int[] actions)
		{
			var isPrevLow = false;
			var lastNonError = -1;

			for (var i = 0; i < actions.Length; i++)
			{
				var action = actions[i];

				if (action < 0)
				{
					if (lastNonError >= 0)
					{
						for (var j = lastNonError + 1; j < i; j++)
						{
							actions[j] = action;
						}
					}

					lastNonError = i;
					isPrevLow = true;
				}
				else if (action > 0)
				{
					if (lastNonError >= 0 && isPrevLow)
					{
						var val = actions[lastNonError];

						for (var j = lastNonError + 1; j < i; j++)
						{
							actions[j] = val;
						}
					}

					lastNonError = i;
					isPrevLow = false;
				}
			}
		}
	}
}
