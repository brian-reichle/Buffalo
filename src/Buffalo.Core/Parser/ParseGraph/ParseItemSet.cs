// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser
{
	[DebuggerDisplay("Count = {_items.Length}")]
	sealed class ParseItemSet : IEquatable<ParseItemSet>, IReadOnlyCollection<ParseItem>, IAppendable
	{
		ParseItemSet(ParseItem[] items)
		{
			_items = items;
			_hash = CalculateHash(items);
			_lookahead = new SegmentSet[items.Length];
		}

		public int Count => _items.Length;
		public bool Contains(ParseItem item) => IndexOf(_items, item) >= 0;

		public SegmentSet GetLookahead(ParseItem item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var index = IndexOf(_items, item);

			if (index < 0) return SegmentSet.EmptySet;
			return _lookahead[index] ?? SegmentSet.EmptySet;
		}

		public void SetLookahead(ParseItem item, SegmentSet lookahead)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var index = IndexOf(_items, item);

			if (index < 0) throw new ArgumentException("item not in this set", nameof(item));
			_lookahead[index] = lookahead;
		}

		public void SubtractLookaheads(SegmentSet lookahead)
		{
			if (lookahead == null) throw new ArgumentNullException(nameof(lookahead));

			for (var i = 0; i < _lookahead.Length; i++)
			{
				_lookahead[i] = _lookahead[i].Subtract(lookahead);
			}
		}

		public bool TryUnionLookahead(ParseItem item, SegmentSet lookahead)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			if (lookahead == null) throw new ArgumentNullException(nameof(lookahead));

			var index = IndexOf(_items, item);

			if (index < 0) throw new ArgumentException("item not in this set", nameof(item));

			var existing = _lookahead[index];

			if (existing == null)
			{
				_lookahead[index] = lookahead;
				return true;
			}
			else if (!existing.IsSupersetOf(lookahead))
			{
				_lookahead[index] = existing.Union(lookahead);
				return true;
			}
			else
			{
				return false;
			}
		}

		public Dictionary<Segment, ParseItemSet> GetTransitionKernels()
		{
			var group = new Dictionary<Segment, List<ParseItem>>();

			for (var i = 0; i < _items.Length; i++)
			{
				var item = _items[i];
				if (item.Position == item.Production.Segments.Length) continue;

				var next = item.Production.Segments[item.Position];

				if (!group.TryGetValue(next, out var list))
				{
					list = new List<ParseItem>();
					group.Add(next, list);
				}

				list.Add(item.NextItem());
			}

			var result = new Dictionary<Segment, ParseItemSet>();

			foreach (var pair in group)
			{
				result.Add(pair.Key, new ParseItemSet(pair.Value.ToArray()));
			}

			return result;
		}

		public override int GetHashCode() => _hash;
		public override bool Equals(object obj) => Equals(obj as ParseItemSet, false);

		public bool Equals(ParseItemSet other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (_hash != other._hash) return false;
			if (_items.Length != other._items.Length) return false;

			for (var i = 0; i < _items.Length; i++)
			{
				if (!_items[i].Equals(other._items[i]))
				{
					return false;
				}
			}

			return true;
		}

		public void AppendTo(StringBuilder builder)
		{
			if (_items.Length > 0)
			{
				Append(builder, _items[0], _lookahead[0]);

				for (var i = 1; i < _items.Length; i++)
				{
					builder.AppendLine();
					Append(builder, _items[i], _lookahead[i]);
				}
			}
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			AppendTo(builder);
			return builder.ToString();
		}

		public static ParseItemSet New(ICollection<ParseItem> items)
		{
			var itemArray = new ParseItem[items.Count];
			items.CopyTo(itemArray, 0);
			Collapse(ref itemArray);
			return new ParseItemSet(itemArray);
		}

		public void CopyTo(ParseItem[] array, int arrayIndex)
			=> Array.Copy(_items, 0, array, arrayIndex, _items.Length);

		public IEnumerator<ParseItem> GetEnumerator()
			=> ((IEnumerable<ParseItem>)_items).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		static void Collapse(ref ParseItem[] result)
		{
			if (result.Length > 1)
			{
				Array.Sort(result, Compare);

				var write = 1;
				for (var read = 1; read < result.Length; read++)
				{
					var item = result[read];

					if (Compare(result[read - 1], item) != 0)
					{
						result[write++] = item;
					}
				}

				Array.Resize(ref result, write);
			}
		}

		static int CalculateHash(ParseItem[] items)
		{
			var result = HashCodeBuilder.Value;

			for (var i = 0; i < items.Length; i++)
			{
				result += items[i].GetHashCode();
			}

			return result;
		}

		static int IndexOf(ParseItem[] items, ParseItem item)
		{
			var lowerLimit = 0;
			var upperLimit = items.Length - 1;

			while (lowerLimit <= upperLimit)
			{
				var mid = (lowerLimit + upperLimit) >> 1;
				var diff = Compare(items[mid], item);

				if (diff < 0)
				{
					lowerLimit = mid + 1;
				}
				else if (diff > 0)
				{
					upperLimit = mid - 1;
				}
				else
				{
					return mid;
				}
			}

			return -1;
		}

		static int Compare(ParseItem i1, ParseItem i2)
		{
			int diff;

			diff = i1.Position - i2.Position;
			if (diff != 0) return diff;

			var p1 = i1.Production;
			var p2 = i2.Production;

			diff = p1.Target.CompareTo(p2.Target);
			if (diff != 0) return diff;

			diff = p1.Segments.Length - p2.Segments.Length;
			if (diff != 0) return diff;

			for (var i = 0; i < p1.Segments.Length; i++)
			{
				diff = p1.Segments[i].CompareTo(p2.Segments[i]);
				if (diff != 0) return diff;
			}

			return 0;
		}

		static void Append(StringBuilder builder, ParseItem item, SegmentSet lookahead)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			builder.Append("[");
			item.Production.Target.AppendTo(builder);
			builder.Append(" ->");

			for (var i = 0; i < item.Position; i++)
			{
				builder.Append(' ');
				item.Production.Segments[i].AppendTo(builder);
			}

			builder.Append(" \u2022");

			for (var i = item.Position; i < item.Production.Segments.Length; i++)
			{
				builder.Append(' ');
				item.Production.Segments[i].AppendTo(builder);
			}

			if (lookahead != null)
			{
				using (var enumerator = lookahead.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						builder.Append(", ");
						enumerator.Current.AppendTo(builder);

						while (enumerator.MoveNext())
						{
							builder.Append("/");
							enumerator.Current.AppendTo(builder);
						}
					}
				}
			}

			builder.Append("]");
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _hash;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly ParseItem[] _items;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly SegmentSet[] _lookahead;
	}
}
