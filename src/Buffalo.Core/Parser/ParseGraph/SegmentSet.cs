// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser
{
	[DebuggerDisplay("Count = {Count}")]
	sealed class SegmentSet : IEquatable<SegmentSet>, ICollection<Segment>, ICollection, IAppendable
	{
		SegmentSet(Segment[] segments)
		{
			_segments = segments;
			_cachedHashCode = GetHashCode(_segments);
		}

		public static SegmentSet EmptySet => _emptySet;
		public static SegmentSet EpsilonSet => _epsilonSet;

		public int Count => _segments.Length;

		public static SegmentSet New(ICollection<Segment> segments)
		{
			if (segments == null) throw new ArgumentNullException(nameof(segments));

			var segmentArray = new Segment[segments.Count];
			segments.CopyTo(segmentArray, 0);
			Sanitise(ref segmentArray);

			return new SegmentSet(segmentArray);
		}

		public SegmentSet Union(SegmentSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			if (Count == 0)
			{
				return otherSet;
			}
			else if (otherSet.Count == 0)
			{
				return this;
			}
			else
			{
				var list = Union(_segments, otherSet._segments);

				if (ReferenceEquals(list, _segments))
				{
					return this;
				}
				else if (ReferenceEquals(list, otherSet._segments))
				{
					return otherSet;
				}
				else
				{
					return new SegmentSet(list);
				}
			}
		}

		public SegmentSet Intersection(SegmentSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			if (Count == 0)
			{
				return this;
			}
			else if (otherSet.Count == 0)
			{
				return otherSet;
			}
			else
			{
				var list = Intersect(_segments, otherSet._segments);

				if (ReferenceEquals(list, _segments))
				{
					return this;
				}
				else if (ReferenceEquals(list, otherSet._segments))
				{
					return otherSet;
				}
				else
				{
					return new SegmentSet(list);
				}
			}
		}

		public SegmentSet Subtract(SegmentSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			if (Count == 0 || otherSet.Count == 0)
			{
				return this;
			}
			else
			{
				var list = Subtract(_segments, otherSet._segments);

				if (ReferenceEquals(list, _segments))
				{
					return this;
				}
				else if (ReferenceEquals(list, otherSet._segments))
				{
					return otherSet;
				}
				else
				{
					return new SegmentSet(list);
				}
			}
		}

		public bool ContainsSegment(Segment segment)
		{
			if (_segments.Length == 0) return false;

			var lower = 0;
			var upper = _segments.Length - 1;

			while (lower <= upper)
			{
				var mid = (upper + lower) >> 1;
				long diff = Compare(segment, _segments[mid]);

				if (diff < 0)
				{
					upper = mid - 1;
				}
				else if (diff > 0)
				{
					lower = mid + 1;
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		public bool Intersects(SegmentSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			return IsOverlapping(_segments, otherSet._segments);
		}

		public bool IsSupersetOf(SegmentSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			return IsSuperSetOf(_segments, otherSet._segments);
		}

		public override int GetHashCode() => _cachedHashCode;
		public override bool Equals(object obj) => Equals(obj as SegmentSet);

		public bool Equals(SegmentSet set)
		{
			if (set == null ||
				set._cachedHashCode != _cachedHashCode ||
				set._segments.Length != _segments.Length) return false;

			for (var i = 0; i < _segments.Length; i++)
			{
				if (_segments[i] != set._segments[i])
				{
					return false;
				}
			}

			return true;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			AppendTo(builder);
			return builder.ToString();
		}

		public void AppendTo(StringBuilder builder)
		{
			if (_segments.Length > 0)
			{
				if (_segments[0] == null)
				{
					builder.Append("null");
				}
				else
				{
					_segments[0].AppendTo(builder);
				}

				for (var i = 1; i < _segments.Length; i++)
				{
					builder.Append(' ');
					_segments[i].AppendTo(builder);
				}
			}
		}

		public void CopyTo(Segment[] array, int arrayIndex)
			=> Array.Copy(_segments, 0, array, arrayIndex, _segments.Length);

		public IEnumerator<Segment> GetEnumerator() => ((IEnumerable<Segment>)_segments).GetEnumerator();

		void ICollection<Segment>.Add(Segment item) => throw new NotSupportedException();
		void ICollection<Segment>.Clear() => throw new NotSupportedException();
		bool ICollection<Segment>.Contains(Segment item) => ContainsSegment(item);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection<Segment>.IsReadOnly => true;
		bool ICollection<Segment>.Remove(Segment item) => throw new NotSupportedException();
		void ICollection.CopyTo(Array array, int index) => _segments.CopyTo(array, index);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => _segments.SyncRoot;
		IEnumerator IEnumerable.GetEnumerator() => _segments.GetEnumerator();

		static int GetHashCode(Segment[] segments)
		{
			var result = HashCodeBuilder.Value;

			for (var i = 0; i < segments.Length; i++)
			{
				result += segments[i]?.GetHashCode();
			}

			return result;
		}

		static void Sanitise(ref Segment[] segments)
		{
			if (segments.Length > 1)
			{
				Array.Sort(segments, Compare);

				var write = 1;
				for (var read = 1; read < segments.Length; read++)
				{
					if (segments[read] != segments[read - 1])
					{
						if (read != write)
						{
							segments[write] = segments[read];
						}

						write++;
					}
				}

				Array.Resize(ref segments, write);
			}
		}

		static Segment[] Union(Segment[] segments1, Segment[] segments2)
		{
			var result = new Segment[segments1.Length + segments2.Length];

			var leftMiss = false;
			var rightMiss = false;

			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < segments1.Length && read2 < segments2.Length)
			{
				var i1 = segments1[read1];
				var i2 = segments2[read2];

				var diff = Compare(i1, i2);

				if (diff < 0)
				{
					result[write++] = i1;
					read1++;
					rightMiss = true;
				}
				else if (diff > 0)
				{
					result[write++] = i2;
					read2++;
					leftMiss = true;
				}
				else
				{
					result[write++] = i1;
					read1++;
					read2++;
				}
			}

			if (read1 < segments1.Length)
			{
				rightMiss = true;

				do
				{
					result[write++] = segments1[read1];
					read1++;
				}
				while (read1 < segments1.Length);
			}
			else if (read2 < segments2.Length)
			{
				leftMiss = true;

				do
				{
					result[write++] = segments2[read2];
					read2++;
				}
				while (read2 < segments2.Length);
			}

			if (!leftMiss)
			{
				return segments1;
			}
			else if (!rightMiss)
			{
				return segments2;
			}
			else
			{
				Array.Resize(ref result, write);
				return result;
			}
		}

		static Segment[] Intersect(Segment[] segments1, Segment[] segments2)
		{
			var result = new Segment[Math.Min(segments1.Length, segments2.Length)];

			var leftMiss = false;
			var rightMiss = false;
			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < segments1.Length && read2 < segments2.Length)
			{
				var i1 = segments1[read1];
				var i2 = segments2[read2];

				var diff = Compare(i1, i2);

				if (diff < 0)
				{
					read1++;
					leftMiss = true;
				}
				else if (diff > 0)
				{
					read2++;
					rightMiss = true;
				}
				else
				{
					result[write++] = i1;
					read1++;
					read2++;
				}
			}

			if (!leftMiss && read1 == segments1.Length)
			{
				return segments1;
			}
			else if (!rightMiss && read2 == segments2.Length)
			{
				return segments2;
			}
			else
			{
				Array.Resize(ref result, write);
				return result;
			}
		}

		static Segment[] Subtract(Segment[] segments1, Segment[] segments2)
		{
			var result = new Segment[segments1.Length];

			var leftMiss = false;
			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < segments1.Length && read2 < segments2.Length)
			{
				var i1 = segments1[read1];
				var i2 = segments2[read2];

				var diff = Compare(i1, i2);

				if (diff < 0)
				{
					result[write++] = i1;
					read1++;
				}
				else if (diff > 0)
				{
					read2++;
				}
				else
				{
					leftMiss = true;
					read1++;
					read2++;
				}
			}

			while (read1 < segments1.Length)
			{
				result[write++] = segments1[read1];
				read1++;
			}

			if (!leftMiss)
			{
				return segments1;
			}
			else
			{
				Array.Resize(ref result, write);
				return result;
			}
		}

		static bool IsOverlapping(Segment[] segments1, Segment[] segments2)
		{
			var read1 = 0;
			var read2 = 0;

			while (read1 < segments1.Length && read2 < segments2.Length)
			{
				var s1 = segments1[read1];
				var s2 = segments2[read2];

				var diff = Compare(s1, s2);

				if (diff < 0)
				{
					read1++;
				}
				else if (diff > 0)
				{
					read2++;
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		static bool IsSuperSetOf(Segment[] segments1, Segment[] segments2)
		{
			var read1 = 0;
			var read2 = 0;

			while (read1 < segments1.Length && read2 < segments2.Length)
			{
				var s1 = segments1[read1];
				var s2 = segments2[read2];

				var diff = Compare(s1, s2);

				if (diff < 0)
				{
					read1++;
				}
				else if (diff > 0)
				{
					return false;
				}
				else
				{
					read1++;
					read2++;
				}
			}

			return read2 == segments2.Length;
		}

		static int Compare(Segment s1, Segment s2)
		{
			if (s1 == null)
			{
				return s2 == null ? 0 : -1;
			}
			else
			{
				return s2 == null ? 1 : s1.CompareTo(s2);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly SegmentSet _emptySet = new SegmentSet(Array.Empty<Segment>());
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly SegmentSet _epsilonSet = new SegmentSet(new Segment[] { null });

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly Segment[] _segments;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _cachedHashCode;
	}
}
