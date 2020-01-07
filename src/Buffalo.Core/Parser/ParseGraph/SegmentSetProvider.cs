// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;

namespace Buffalo.Core.Parser
{
	sealed class SegmentSetProvider
	{
		public SegmentSetProvider(Dictionary<Segment, Production[]> productions)
		{
			_productions = productions;
			_firstsSet = ExtractFirstsSet(productions);
			_followSets = ExtractFollowSets(productions, _firstsSet);
			InitialSegments = ExtractInitialSegments(productions);
		}

		public SegmentSet InitialSegments { get; }

		public SegmentSet GetFirstsSet(Segment segment)
		{
			if (!_firstsSet.TryGetValue(segment, out var result))
			{
				return SegmentSet.EmptySet;
			}
			else
			{
				return result;
			}
		}

		public SegmentSet GetFollowSets(Production production, int position)
		{
			if (!_followSets.TryGetValue(production, out var setList))
			{
				return SegmentSet.EmptySet;
			}
			else
			{
				return setList[position];
			}
		}

		public ParseItem[] CreateParseItems(Segment nonTerminal)
		{
			if (!_productions.TryGetValue(nonTerminal, out var productions))
			{
				return System.Array.Empty<ParseItem>();
			}
			else
			{
				var result = new ParseItem[productions.Length];

				for (var i = 0; i < result.Length; i++)
				{
					result[i] = new ParseItem(productions[i], 0);
				}

				return result;
			}
		}

		static Dictionary<Segment, SegmentSet> ExtractFirstsSet(Dictionary<Segment, Production[]> productions)
		{
			var changed = true;
			var result = new Dictionary<Segment, SegmentSet>();

			while (changed)
			{
				changed = false;

				foreach (var nonTerminal in productions)
				{
					if (!result.TryGetValue(nonTerminal.Key, out var firsts))
					{
						firsts = SegmentSet.EmptySet;
					}

					for (var i = 0; i < nonTerminal.Value.Length; i++)
					{
						var production = nonTerminal.Value[i];
						var containsEmpty = true;

						for (var k = 0; k < production.Segments.Length; k++)
						{
							SegmentSet set;
							SegmentSet segmentsToAdd;
							var pSegment = production.Segments[k];

							if (pSegment.IsTerminal)
							{
								segmentsToAdd = set = SegmentSet.New(new Segment[] { pSegment });
							}
							else if (result.TryGetValue(pSegment, out set))
							{
								segmentsToAdd = set.Subtract(SegmentSet.EpsilonSet);
							}
							else
							{
								segmentsToAdd = set = SegmentSet.EmptySet;
							}

							if (!firsts.IsSupersetOf(segmentsToAdd))
							{
								firsts = firsts.Union(segmentsToAdd);
								changed = true;
							}

							if (!set.ContainsSegment(null))
							{
								containsEmpty = false;
								break;
							}
						}

						if (containsEmpty && !firsts.ContainsSegment(null))
						{
							firsts = firsts.Union(SegmentSet.EpsilonSet);
							changed = true;
						}
					}

					result[nonTerminal.Key] = firsts;
				}
			}

			return result;
		}

		static Dictionary<Production, SegmentSet[]> ExtractFollowSets(Dictionary<Segment, Production[]> productions, Dictionary<Segment, SegmentSet> firstsSet)
		{
			var result = new Dictionary<Production, SegmentSet[]>();

			foreach (var pair in productions)
			{
				foreach (var prod in pair.Value)
				{
					if (prod.Segments.Length > 0)
					{
						result[prod] = ExtractFollowSets(prod, firstsSet);
					}
				}
			}

			return result;
		}

		static SegmentSet[] ExtractFollowSets(Production prod, Dictionary<Segment, SegmentSet> firstsSet)
		{
			var sets = new SegmentSet[prod.Segments.Length];

			if (prod.Segments.Length > 0)
			{
				var trailing = SegmentSet.EpsilonSet;

				for (var i = sets.Length - 1; i > 0; i--)
				{
					sets[i] = trailing;

					var seg = prod.Segments[i];

					if (seg.IsTerminal)
					{
						trailing = SegmentSet.New(new Segment[] { seg });
					}
					else
					{
						if (firstsSet.TryGetValue(seg, out var set))
						{
							if (set.ContainsSegment(null))
							{
								trailing = trailing.Union(set.Subtract(SegmentSet.EpsilonSet));
							}
							else
							{
								trailing = set;
							}
						}
					}
				}

				sets[0] = trailing;
			}

			return sets;
		}

		static SegmentSet ExtractInitialSegments(Dictionary<Segment, Production[]> productions)
		{
			var list = new List<Segment>();

			foreach (var pair in productions)
			{
				if (pair.Key.IsInitial)
				{
					list.Add(pair.Key);
				}
			}

			return list.Count == 0 ? SegmentSet.EmptySet : SegmentSet.New(list);
		}

		readonly Dictionary<Segment, Production[]> _productions;
		readonly Dictionary<Segment, SegmentSet> _firstsSet;
		readonly Dictionary<Production, SegmentSet[]> _followSets;
	}
}
