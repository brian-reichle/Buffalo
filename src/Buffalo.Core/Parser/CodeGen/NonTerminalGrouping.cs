// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	static class NonTerminalGrouping
	{
		public static Segment[][] GroupSegments(Graph graph)
		{
			var signatures = ExtractSignatures(graph);
			SortSignatures(signatures);
			MergeSignatures(ref signatures);
			SortSignatures(signatures);

			var groups = new Segment[signatures.Length][];

			for (var n = 0; n < groups.Length; n++)
			{
				var sig = signatures[n];
				groups[n] = sig.Segments.ToArray();
			}

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
					if (seg.IsTerminal) continue;

					if (!signatures.TryGetValue(seg, out var sig))
					{
						sig = new Signature(stateCount);
						sig.Segments.Add(seg);
						signatures.Add(seg, sig);
					}

					sig.GotoTargets[index] = transition.ToState;
				}

				index++;
			}

			var result = new Signature[signatures.Count];
			signatures.Values.CopyTo(result, 0);

			return result;
		}

		static void SortSignatures(Signature[] signatures)
		{
			var key = new int[signatures.Length];

			for (var n = 0; n < signatures.Length; n++)
			{
				key[n] = -Count(signatures[n]);
			}

			Array.Sort(key, signatures);
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

			var s = source.GotoTargets;
			var t = target.GotoTargets;

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

			for (var n = 0; n < sig.GotoTargets.Length; n++)
			{
				if (sig.GotoTargets[n] != null)
				{
					count++;
				}
			}

			return count;
		}

		static bool Equal(Signature sig1, Signature sig2) => Equal(sig1.GotoTargets, sig2.GotoTargets);

		static bool Equal(Graph.State[] stateSet1, Graph.State[] stateSet2)
		{
			for (var n = 0; n < stateSet1.Length; n++)
			{
				var s1 = stateSet1[n];
				var s2 = stateSet2[n];

				if (s1 != null && s2 != null && s1 != s2) return false;
			}

			return true;
		}

		sealed class Signature
		{
			public Signature(int stateCount)
			{
				Segments = new List<Segment>();
				GotoTargets = new Graph.State[stateCount];
			}

			public List<Segment> Segments { get; }
			public Graph.State[] GotoTargets { get; }
		}
	}
}
