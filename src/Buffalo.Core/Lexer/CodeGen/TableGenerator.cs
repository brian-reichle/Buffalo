// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using Buffalo.Core.Common;
using Buffalo.Core.Lexer.Configuration;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	static class TableGenerator
	{
		public static TableData ExtractData(Config config, ConfigTable table)
		{
			var charSets = FATools.ExtractAlphabet(table.Graph);
			var charClassMap = GetCharClassMap(table.Graph, charSets);
			var stateMap = GetStateMap(table.Graph);

			var stateCount = table.Graph.States.Count;

			var statistics = new Statistics()
			{
				CharClassifications = charSets.Length,
				States = stateCount,
			};

			var data = new TableData()
			{
				TableID = table.Index,
				Statistics = statistics,
			};

			{
				var combined = ExtractTransitionTable(table, charSets, charClassMap, stateMap, statistics);

				var offsetsSectionLen = stateCount;
				var transitionTable = new int[offsetsSectionLen + combined.Count];
				combined.CopyTo(transitionTable, offsetsSectionLen);

				for (var i = 0; i < stateCount; i++)
				{
					var offset = combined.GetOffset(i);

					if (offset.HasValue)
					{
						transitionTable[i] = offset.Value + offsetsSectionLen;
					}
				}

				var transitionsBlob = CompressedBlob.Compress(config.Manager.TableCompression, ElementSizeStrategy.Get(config.Manager.ElementSize), transitionTable);

				statistics.TransitionsRunTime = transitionTable.Length;
				statistics.TransitionsAssemblyBytes = transitionsBlob.Bytes;
				data.TransitionTable = transitionsBlob;
			}

			{
				var ranges = GetRanges(charSets, charClassMap);
				ExtractClasificationTable(ranges, out var boundries, out var classifications);

				statistics.CharRanges = ranges.Length;
				data.CharClassificationBoundries = boundries;
				data.CharClassification = classifications;
				data.StateMap = stateMap;
			}

			return data;
		}

		static void ExtractClasificationTable(KeyValuePair<char, int>[] ranges, out char[] boundries, out int[] classifications)
		{
			boundries = new char[ranges.Length - 1];
			classifications = new int[ranges.Length];

			var i = ranges.Length - 1;
			classifications[i] = ranges[i].Value;
			i--;

			while (i >= 0)
			{
				boundries[i] = ranges[i].Key;
				classifications[i] = ranges[i].Value;
				i--;
			}
		}

		static TableFragment ExtractTransitionTable(ConfigTable table, CharSet[] charSets, int[] charClassMap, Dictionary<Graph.State, int> stateMap, Statistics statistics)
		{
			var fragments = new List<TableFragment>();

			foreach (var fromState in table.Graph.States)
			{
				int[] transitionsList = null;

				for (var i = 0; i < charSets.Length; i++)
				{
					var toState = NextState(fromState, charSets[charClassMap[i]]);

					if (toState != null)
					{
						if (transitionsList == null)
						{
							transitionsList = new int[charSets.Length];
						}

						transitionsList[i] = stateMap[toState] + 1;
					}
				}

				if (transitionsList != null)
				{
					fragments.Add(new TableFragment(transitionsList, stateMap[fromState]));
				}
				else
				{
					statistics.StatesTerminal++;
				}
			}

			return TableFragment.Combine(fragments);
		}

		/// <summary>
		/// Generate an array that maps char classes to transition column indices.
		/// </summary>
		/// <remarks>
		/// This method attemps to place the more frequently used terminals in the middle of the rows,
		/// this increases the effectiveness of row overlapping by increasing the number of values at
		/// the end of the rows that are likely to overlap with other rows (the error value, '0', is
		/// by far the most likely to overlap with other rows.
		/// </remarks>
		static int[] GetCharClassMap(Graph graph, CharSet[] charClasses)
		{
			var map = new int[charClasses.Length];
			var usageCounts = new int[map.Length];

			for (var i = 0; i < charClasses.Length; i++)
			{
				var set = charClasses[i];
				var count = 0;

				foreach (var state in graph.States)
				{
					foreach (var transition in state.ToTransitions)
					{
						if (set.Intersects(transition.Label))
						{
							count++;
						}
					}
				}

				map[i] = i;
				usageCounts[i] = count;
			}

			Array.Sort(usageCounts, map);

			var result = new int[map.Length];

			for (var i = 0; i < map.Length; i++)
			{
				var j = (i & 1) == 0 ? (i >> 1) : result.Length - (i >> 1) - 1;
				result[j] = map[i];
			}

			return result;
		}

		/// <summary>
		/// Generate an array that maps states to transition row indices.
		/// </summary>
		/// <remarks>
		/// This method attempts to group states with the same result together.
		/// </remarks>
		static Dictionary<Graph.State, int> GetStateMap(Graph graph)
		{
			var states = new Graph.State[graph.States.Count];
			var types = new int[graph.States.Count];

			for (var i = 0; i < graph.States.Count; i++)
			{
				var state = graph.States[i];

				states[i] = state;
				types[i] = state.Label.EndState.GetValueOrDefault(-1);
			}

			Array.Sort(types, states);

			var result = new Dictionary<Graph.State, int>();

			for (var i = 0; i < states.Length; i++)
			{
				result[states[i]] = i;
			}

			return result;
		}

		static Graph.State NextState(Graph.State state, CharSet charSet)
		{
			foreach (var transition in state.ToTransitions)
			{
				if (charSet.Intersects(transition.Label))
				{
					return transition.ToState;
				}
			}

			return null;
		}

		static KeyValuePair<char, int>[] GetRanges(CharSet[] charSets, int[] charClassMap)
		{
			var list = new List<KeyValuePair<char, int>>();

			for (var i = 0; i < charSets.Length; i++)
			{
				var set = charSets[charClassMap[i]];

				foreach (var range in set)
				{
					list.Add(new KeyValuePair<char, int>(range.To, i));
				}
			}

			list.Sort((r1, r2) => r1.Key - r2.Key);

			return list.ToArray();
		}
	}
}
