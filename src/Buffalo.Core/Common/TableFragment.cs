// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	/// <remarks>
	/// A two dimentional array, or table, can be represented by a single dimentional table
	/// where the data for position (x, y) is stored at index x + table_width * y. If the
	/// table is read-only then the multiplication can be replaced with an index lookup
	/// allowing identical portions of the table to be overlapped thus (depending on the
	/// data) reducing the memory footprint of the table.
	///
	/// The purpose of this class is to take a bunch of rows and try to find an arangement
	/// with a good aproximation of the minimal space required. Finding the optimal solution
	/// reduces to the "Traveling Sailsman Problem", so a good aproximation is sufficent.
	///
	/// This particular algorithm is intended to work with the characteristics of the sparse
	/// state transition tables of the lexer/parser and will probably perform very poorly
	/// for other cases.
	///
	/// Algorithm Overview:
	///
	/// * If two fragments with the same content are encountered, they are combined with
	///   identical offsets.
	///
	/// * Each fragment gets two tags, one for each end. all start tags are put into one list
	///   and all end tags in another.
	///
	/// * Sort both lists first by "end value" (the very first value for a start tag and the
	///   very last value for an end tag) and second by "end length" (the number of
	///   consecitive times "end value" appears) in decending order.
	///
	/// * Make a single pass over both lists at the same time, combining distinct fragments
	///   with the longest sequence of an identical value.
	///
	/// * All remaining fragments are appended end to end.
	/// </remarks>
	[DebuggerTypeProxy(typeof(TypeProxy))]
	[DebuggerDisplay("Count = {Count}")]
	sealed class TableFragment : IList<int>, ICollection
	{
		public TableFragment(int[] values, int state)
			: this(values, new int[] { state }, new int[] { 0 }, 0)
		{
		}

		public TableFragment(int[] values, int state, int skip)
			: this(values, new int[] { state }, new int[] { -skip }, skip)
		{
		}

		TableFragment(int[] values, int[] states, int[] offsets, int skip)
		{
			Skip = skip;
			_values = values;
			_states = states;
			_offsets = offsets;
		}

		/// <summary>
		/// Attempt to combine multiple TableFragments into a single fragment.
		/// </summary>
		/// <remarks>
		/// This method attempts to minimise the total length of the result by adjusting the
		/// offsets so that identical portions overlap.
		/// </remarks>
		public static TableFragment Combine(IList<TableFragment> fragments)
		{
			if (fragments == null) throw new ArgumentNullException(nameof(fragments));
			if (fragments.Count == 0) throw new ArgumentException("fragments list cannot be empty", nameof(fragments));

			var startTags = new List<TableFragmentTag>(fragments.Count);
			var endTags = new List<TableFragmentTag>(fragments.Count);

			var map = new Dictionary<int, List<TableFragmentTag>>();

			for (var i = 0; i < fragments.Count; i++)
			{
				CreateTags(fragments[i], out var startTag, out var endTag, out var hash);

				if (!map.TryGetValue(hash, out var list))
				{
					list = new List<TableFragmentTag>();
					list.Add(startTag);
					map.Add(hash, list);

					startTags.Add(startTag);
					endTags.Add(endTag);
				}
				else if (!AddEquivilenceList(list, startTag))
				{
					startTags.Add(startTag);
					endTags.Add(endTag);
				}
			}

			startTags.Sort(Comparison);
			endTags.Sort(Comparison);

			var startFragments = new LinkedList<TableFragmentTag>(startTags);
			var endFragments = new LinkedList<TableFragmentTag>(endTags);

			return Combine(startFragments, endFragments);
		}

		/// <summary>
		/// Indicates the number of "don't care" elements between the offset origin and the first value.
		/// </summary>
		/// <remarks>
		/// This is useful if it is known that the first N elements can never be used, and as such can have
		/// any value.
		/// </remarks>
		public int Skip { get; }

		public int this[int index] => _values[index];

		/// <summary>
		/// Gets the offset into this fragment where the values associated with the given state can be found.
		/// </summary>
		/// <remarks>
		/// Returns null if the state was never combined into this fragment.
		/// </remarks>
		public int? GetOffset(int state)
		{
			var index = Array.BinarySearch(_states, state);
			return index < 0 ? new int?() : _offsets[index];
		}

		static void CreateTags(TableFragment fragment, out TableFragmentTag startTag, out TableFragmentTag endTag, out int hash)
		{
			if (fragment == null) throw new ArgumentNullException(nameof(fragment));

			var values = fragment._values;

			var startIndex = 0;
			var startValue = values[0];
			var endIndex = 0;
			var endValue = values[values.Length - 1];
			uint currentHash = 0;
			var startSet = false;

			for (var i = 0; i < values.Length; i++)
			{
				var val = values[i];

				if (!startSet && val != startValue)
				{
					startSet = true;
					startIndex = i;
				}

				if (val != endValue)
				{
					endIndex = i;
				}

				currentHash = unchecked(((currentHash >> 5) | (currentHash << 27)) ^ (uint)val);
			}

			startTag = new TableFragmentTag()
			{
				Fragment = fragment,
				EndLen = startSet ? startIndex : values.Length,
				EndValue = startValue,
			};

			endTag = new TableFragmentTag()
			{
				Fragment = fragment,
				EndLen = startSet ? values.Length - endIndex - 1 : values.Length,
				EndValue = endValue,
			};

			startTag.Partner = endTag;
			endTag.Partner = startTag;
			hash = (int)currentHash;
		}

		/// <summary>
		/// Replace the TableFragments referenced by the two tags with a single fragment that
		/// contains their combined values.
		/// </summary>
		/// <remarks>
		/// The end of tag1 will be made to overlap the start of tag2 as far as possible.
		/// </remarks>
		static void Combine(TableFragmentTag tag1, TableFragmentTag tag2)
		{
			int overlap;

			if (tag1.EndValue != tag2.EndValue)
			{
				overlap = 0;
			}
			else
			{
				overlap = Math.Min(tag1.EndLen, tag2.EndLen);
			}

			var fragment = Combine(tag1.Fragment, tag2.Fragment, overlap);

			tag1.Partner.Fragment = fragment;
			tag2.Partner.Fragment = fragment;
			tag1.Partner.Partner = tag2.Partner;
			tag2.Partner.Partner = tag1.Partner;
		}

		static TableFragment Combine(TableFragment fragment1, TableFragment fragment2, int overlap)
		{
			var values1 = fragment1._values;
			var values2 = fragment2._values;
			var states1 = fragment1._states;
			var states2 = fragment2._states;
			var offsets1 = fragment1._offsets;
			var offsets2 = fragment2._offsets;

			var offset = values1.Length - overlap;
			var values = new int[offset + values2.Length];
			var states = new int[states1.Length + states2.Length];
			var offsets = new int[states.Length];

			Array.Copy(values1, values, offset);
			Array.Copy(values2, 0, values, offset, values2.Length);

			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < states1.Length && read2 < states2.Length)
			{
				var diff = states1[read1] - states2[read2];

				if (diff == 0)
					throw new InvalidOperationException("attempting to combine 2 fragments that both contain the state " + states1[read1]);

				if (diff < 0)
				{
					states[write] = states1[read1];
					offsets[write] = offsets1[read1];
					write++;
					read1++;
				}
				else
				{
					states[write] = states2[read2];
					offsets[write] = offsets2[read2] + offset;
					write++;
					read2++;
				}
			}

			while (read1 < states1.Length)
			{
				states[write] = states1[read1];
				offsets[write] = offsets1[read1];
				write++;
				read1++;
			}

			while (read2 < states2.Length)
			{
				states[write] = states2[read2];
				offsets[write] = offsets2[read2] + offset;
				write++;
				read2++;
			}

			return new TableFragment(values, states, offsets, Math.Max(fragment1.Skip, fragment2.Skip - offset));
		}

		static TableFragment Combine(LinkedList<TableFragmentTag> startFragmentTags, LinkedList<TableFragmentTag> endFragmentTags)
		{
			var finalFragments = new LinkedList<TableFragmentTag>();

			while (startFragmentTags.Count > 0 && endFragmentTags.Count > 0)
			{
				var tailNode = startFragmentTags.First;
				var leadNode = endFragmentTags.First;

				if (tailNode.Value.Fragment == leadNode.Value.Fragment)
				{
					/*
					 * if the tailNode and leadNode reference the same fragment, then try to replace one
					 * of them as bad things happen when overlapping a fragment with itself.
					 */

					if (leadNode.Next != null)
					{
						leadNode = leadNode.Next;
					}
					else if (tailNode.Next != null)
					{
						tailNode = tailNode.Next;
					}
					else
					{
						break;
					}
				}

				var leadEnd = leadNode.Value.EndValue;
				var tailStart = tailNode.Value.EndValue;

				if (leadEnd < tailStart)
				{
					endFragmentTags.Remove(leadNode);
				}
				else if (tailStart < leadEnd)
				{
					startFragmentTags.Remove(tailNode);
					finalFragments.AddLast(tailNode);
				}
				else
				{
					endFragmentTags.Remove(leadNode);
					startFragmentTags.Remove(tailNode);
					TableFragment.Combine(leadNode.Value, tailNode.Value);
				}
			}

			while (startFragmentTags.Count > 0)
			{
				var node = startFragmentTags.First;
				startFragmentTags.Remove(node);
				finalFragments.AddLast(node);
			}

			return ForceCombine(finalFragments);
		}

		/// <summary>
		/// Append all fragments end to end without any attempt to overlap any of them.
		/// </summary>
		/// <remarks>
		/// Used as a last resort for when all attempts to overlap the fragments have failed.
		/// </remarks>
		static TableFragment ForceCombine(LinkedList<TableFragmentTag> finalFragments)
		{
			var totalLen = 0;
			var totalStates = 0;

			foreach (var tag in finalFragments)
			{
				var fragment = tag.Fragment;
				totalLen += fragment._values.Length;
				totalStates += fragment._states.Length;
			}

			var newSkip = 0;
			var newValues = new int[totalLen];
			var newStates = new int[totalStates];
			var newOffsets = new int[totalStates];

			totalLen = 0;
			totalStates = 0;

			foreach (var tag in finalFragments)
			{
				var fragment = tag.Fragment;
				var values = fragment._values;
				var states = fragment._states;
				var offsets = fragment._offsets;

				newSkip = Math.Max(newSkip, tag.Fragment.Skip - totalLen);

				Array.Copy(states, 0, newStates, totalStates, states.Length);
				Array.Copy(values, 0, newValues, totalLen, values.Length);

				for (var i = 0; i < states.Length; i++)
				{
					newOffsets[i + totalStates] = offsets[i] + totalLen;
				}

				totalLen += values.Length;
				totalStates += states.Length;
			}

			Array.Sort(newStates, newOffsets);

			return new TableFragment(newValues, newStates, newOffsets, newSkip);
		}

		/// <remarks>
		/// check for fragments with the same values, if one is found then combine them, otherwise add startTag to the list.
		/// </remarks>
		static bool AddEquivilenceList(List<TableFragmentTag> list, TableFragmentTag startTag)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var existingTag = list[i];
				var existing = existingTag.Fragment;
				var fragment = startTag.Fragment;

				if (ValuesEquivilent(existing._values, fragment._values))
				{
					existingTag.Partner.Fragment = existingTag.Fragment = Combine(existing, fragment, existing.Count);
					return true;
				}
			}

			list.Add(startTag);
			return false;
		}

		static bool ValuesEquivilent(int[] values1, int[] values2)
		{
			if (values1.Length != values2.Length) return false;

			for (var i = 0; i < values1.Length; i++)
			{
				if (values1[i] != values2[i]) return false;
			}

			return true;
		}

		static int Comparison(TableFragmentTag tag1, TableFragmentTag tag2)
		{
			var diff = tag1.EndValue - tag2.EndValue;
			return diff != 0 ? diff : tag2.EndLen - tag1.EndLen;
		}

		public int Count => _values.Length;
		public void CopyTo(int[] array, int arrayIndex) => Array.Copy(_values, 0, array, arrayIndex, _values.Length);
		public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)_values).GetEnumerator();

		int IList<int>.IndexOf(int item) => Array.IndexOf<int>(_values, item);
		void IList<int>.Insert(int index, int item) => throw new NotSupportedException();
		void IList<int>.RemoveAt(int index) => throw new NotSupportedException();

		int IList<int>.this[int index]
		{
			get => _values[index];
			set => throw new NotSupportedException();
		}

		void ICollection<int>.Add(int item) => throw new NotSupportedException();
		void ICollection<int>.Clear() => throw new NotSupportedException();
		bool ICollection<int>.Contains(int item) => Array.IndexOf(_values, item) >= 0;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection<int>.IsReadOnly => true;
		bool ICollection<int>.Remove(int item) => throw new NotSupportedException();

		void ICollection.CopyTo(Array array, int index) => Array.Copy(_values, array, index);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => _values.SyncRoot;

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		sealed class TypeProxy
		{
			public TypeProxy(TableFragment fragment)
			{
				_elements = new TypeProxyElement[fragment.Count];
				var empty = Array.Empty<int>();

				var states = new Dictionary<int, List<int>>();
				for (var i = 0; i < fragment._states.Length; i++)
				{
					var offset = fragment._offsets[i];

					if (!states.TryGetValue(offset, out var list))
					{
						list = new List<int>();
						states.Add(offset, list);
					}

					list.Add(fragment._states[i]);
				}

				for (var i = 0; i < fragment.Count; i++)
				{
					states.TryGetValue(i, out var list);

					_elements[i] = new TypeProxyElement(fragment[i], list == null ? empty : list.ToArray());
				}

				Skip = fragment.Skip;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
			public int Skip { get; }
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			readonly TypeProxyElement[] _elements;
		}

		[DebuggerDisplay("{Value}")]
		sealed class TypeProxyElement
		{
			public TypeProxyElement(int value, int[] states)
			{
				Value = value;
				States = states;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Value { get; }
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public int[] States { get; }
		}

		/// <summary>
		/// The TableFragmentTag class is a reference to one end of a TableFragment, either the start or the end.
		/// </summary>
		[DebuggerDisplay("{EndValue} x {EndLen}")]
		sealed class TableFragmentTag
		{
			/// <summary>
			/// The Fragment this tag references.
			/// </summary>
			public TableFragment Fragment { get; set; }

			/// <summary>
			/// The tag for the other end of the fragment.
			/// </summary>
			public TableFragmentTag Partner { get; set; }

			/// <summary>
			/// The value at this end of the fragment.
			/// </summary>
			public int EndValue { get; set; }

			/// <summary>
			/// The number of consecitive times EndValue appears.
			/// </summary>
			public int EndLen { get; set; }
		}

		readonly int[] _values;
		readonly int[] _states;
		readonly int[] _offsets;
	}
}
