// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	static class TerminalGrouping
	{
		public static Segment[][] GroupSegments(Graph graph)
		{
			var signatures = ExtractSignatures(graph);
			MergeSignatures(ref signatures);

			var groups = new Segment[signatures.Length][];
			var counts = new int[signatures.Length];

			for (var n = 0; n < groups.Length; n++)
			{
				var sig = signatures[n];
				groups[n] = sig.Segments.ToArray();
				counts[n] = Count(sig);
			}

			Array.Sort(counts, groups);

			return groups;
		}

		static Signature[] ExtractSignatures(Graph graph)
		{
			var signatures = new Dictionary<Segment, Signature>();

			var index = 0;
			var stateCount = graph.States.Count;

			foreach (var state in graph.States)
			{
				foreach (var transition in state.ToTransitions)
				{
					var seg = transition.Label;
					if (!seg.IsTerminal) continue;

					if (!signatures.TryGetValue(seg, out var sig))
					{
						sig = new Signature(stateCount);
						sig.Segments.Add(seg);
						signatures.Add(seg, sig);
					}

					sig.ShiftTargets[index] = transition.ToState;
				}

				foreach (var item in state.Label)
				{
					var production = item.Production;
					if (item.Position < production.Segments.Length) continue;

					foreach (var seg in state.Label.GetLookahead(item))
					{
						if (!signatures.TryGetValue(seg, out var sig))
						{
							sig = new Signature(stateCount);
							sig.Segments.Add(seg);
							signatures.Add(seg, sig);
						}

						sig.ProductionTargets[index] = production;
					}
				}

				index++;
			}

			var result = new Signature[signatures.Count];
			signatures.Values.CopyTo(result, 0);

			for (var n = 0; n < result.Length; n++)
			{
				var shiftTargets = result[n].ShiftTargets;

				uint hash = 0;

				for (var m = 0; m < shiftTargets.Length; m++)
				{
					hash = hash << 27 | hash >> 5;

					if (shiftTargets[m] != null)
					{
						hash ^= unchecked((uint)shiftTargets[m].GetHashCode());
					}
				}

				result[n].Hash = unchecked((int)hash);
			}

			return result;
		}

		static void MergeSignatures(ref Signature[] signatures)
		{
			if (signatures.Length > 1)
			{
				var write = 1;
				for (var read = 1; read < signatures.Length; read++)
				{
					var merged = false;

					for (var i = 0; i < write; i++)
					{
						if (!Equal(signatures[read], signatures[i])) continue;

						MergeInto(signatures[read], signatures[i]);
						merged = true;
						break;
					}

					if (!merged)
					{
						signatures[write++] = signatures[read];
					}
				}

				Array.Resize(ref signatures, write);
			}
		}

		static void MergeInto(Signature source, Signature target)
		{
			target.Segments.AddRange(source.Segments);

			var s = source.ProductionTargets;
			var t = target.ProductionTargets;

			for (var n = 0; n < s.Length; n++)
			{
				if (s[n] != null)
				{
					t[n] = s[n];
				}
			}
		}

		static int Count(Signature sig)
		{
			var count = 0;

			for (var n = 0; n < sig.ShiftTargets.Length; n++)
			{
				if (sig.ShiftTargets[n] != null || sig.ProductionTargets[n] != null)
				{
					count++;
				}
			}

			return count;
		}

		static bool Equal(Signature sig1, Signature sig2)
		{
			return sig1.Hash == sig2.Hash
				&& Equal(sig1.ShiftTargets, sig2.ShiftTargets)
				&& Equal(sig1.ProductionTargets, sig2.ProductionTargets);
		}

		static bool Equal(Graph.State[] stateSet1, Graph.State[] stateSet2)
		{
			for (var n = 0; n < stateSet1.Length; n++)
			{
				if (stateSet1[n] != stateSet2[n]) return false;
			}

			return true;
		}

		static bool Equal(Production[] prodSet1, Production[] prodSet2)
		{
			for (var n = 0; n < prodSet1.Length; n++)
			{
				if (prodSet1[n] != null &&
					prodSet2[n] != null &&
					prodSet1[n] != prodSet2[n])
				{
					return false;
				}
			}

			return true;
		}

		sealed class Signature
		{
			public Signature(int stateCount)
			{
				Segments = new List<Segment>();
				ShiftTargets = new Graph.State[stateCount];
				ProductionTargets = new Production[stateCount];
			}

			public int Hash { get; set; }
			public List<Segment> Segments { get; }
			public Graph.State[] ShiftTargets { get; }
			public Production[] ProductionTargets { get; }
		}
	}
}
