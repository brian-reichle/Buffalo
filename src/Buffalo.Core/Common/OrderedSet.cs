// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	[DebuggerDisplay("Count = {_values.Length}")]
	sealed class OrderedSet<T> : IEquatable<OrderedSet<T>>, IEnumerable<T>, ICollection
		where T : IComparable<T>
	{
		OrderedSet(T[] values)
		{
			_values = values;
			_cachedHashCode = GetHashCode(_values);
		}

		public static OrderedSet<T> New(ICollection<T> values)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));

			var valuesArray = new T[values.Count];
			values.CopyTo(valuesArray, 0);
			Sanitise(ref valuesArray);

			return new OrderedSet<T>(valuesArray);
		}

		public static OrderedSet<T> New(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return new OrderedSet<T>(new T[] { value });
		}

		public int Length => _values.Length;
		public override int GetHashCode() => _cachedHashCode;
		public override bool Equals(object obj) => Equals(obj as OrderedSet<T>);

		public bool Equals(OrderedSet<T> other)
		{
			if (other == null ||
				other._cachedHashCode != _cachedHashCode ||
				other._values.Length != _values.Length) return false;

			for (var i = 0; i < _values.Length; i++)
			{
				if (_values[i].CompareTo(other._values[i]) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public OrderedSet<T> Union(OrderedSet<T> otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			if (Length == 0)
			{
				return otherSet;
			}
			else if (otherSet.Length == 0)
			{
				return this;
			}
			else
			{
				return new OrderedSet<T>(Union(_values, otherSet._values));
			}
		}

		public OrderedSet<T> Intersection(OrderedSet<T> otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			if (Length == 0)
			{
				return this;
			}
			else if (otherSet.Length == 0)
			{
				return otherSet;
			}
			else
			{
				return new OrderedSet<T>(Intersect(_values, otherSet._values));
			}
		}

		public OrderedSet<T> Subtract(OrderedSet<T> otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			if (Length == 0 || otherSet.Length == 0)
			{
				return this;
			}
			else
			{
				return new OrderedSet<T>(Subtract(_values, otherSet._values));
			}
		}

		public bool ContainsValue(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (_values.Length == 0) return false;

			var lower = 0;
			var upper = _values.Length - 1;

			while (lower <= upper)
			{
				var mid = (upper + lower) >> 1;
				long diff = value.CompareTo(_values[mid]);

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

		public bool Intersects(OrderedSet<T> otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			return IsOverlapping(_values, otherSet._values);
		}

		public bool IsSupersetOf(OrderedSet<T> otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));

			return IsSuperSetOf(_values, otherSet._values);
		}

		public void CopyTo(T[] array, int index) => ((ICollection<T>)_values).CopyTo(array, index);
		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_values).GetEnumerator();

		void ICollection.CopyTo(Array array, int index) => _values.CopyTo(array, index);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int ICollection.Count => Length;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => this;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		static int GetHashCode(T[] values)
		{
			var result = HashCodeBuilder.Value;

			for (var i = 0; i < values.Length; i++)
			{
				result += values[i].GetHashCode();
			}

			return result;
		}

		static void Sanitise(ref T[] values)
		{
			if (values.Length > 1)
			{
				Array.Sort(values);

				var write = 1;
				for (var read = 1; read < values.Length; read++)
				{
					if (values[read].CompareTo(values[read - 1]) != 0)
					{
						if (read != write)
						{
							values[write] = values[read];
						}

						write++;
					}
				}

				Array.Resize(ref values, write);
			}
		}

		static T[] Union(T[] values1, T[] values2)
		{
			var result = new T[values1.Length + values2.Length];

			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < values1.Length && read2 < values2.Length)
			{
				var i1 = values1[read1];
				var i2 = values2[read2];

				var diff = i1.CompareTo(i2);

				if (diff < 0)
				{
					result[write++] = i1;
					read1++;
				}
				else if (diff > 0)
				{
					result[write++] = i2;
					read2++;
				}
				else
				{
					result[write++] = i1;
					read1++;
					read2++;
				}
			}

			if (read1 < values1.Length)
			{
				do
				{
					result[write++] = values1[read1];
					read1++;
				}
				while (read1 < values1.Length);
			}
			else
			{
				while (read2 < values2.Length)
				{
					result[write++] = values2[read2];
					read2++;
				}
			}

			Array.Resize(ref result, write);
			return result;
		}

		static T[] Intersect(T[] values1, T[] values2)
		{
			var result = new T[Math.Min(values1.Length, values2.Length)];

			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < values1.Length && read2 < values2.Length)
			{
				var i1 = values1[read1];
				var i2 = values2[read2];

				var diff = i1.CompareTo(i2);

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
					result[write++] = i1;
					read1++;
					read2++;
				}
			}

			Array.Resize(ref result, write);
			return result;
		}

		static T[] Subtract(T[] values1, T[] values2)
		{
			var result = new T[values1.Length];

			var read1 = 0;
			var read2 = 0;
			var write = 0;

			while (read1 < values1.Length && read2 < values2.Length)
			{
				var i1 = values1[read1];
				var i2 = values2[read2];

				var diff = i1.CompareTo(i2);

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
					read1++;
					read2++;
				}
			}

			while (read1 < values1.Length)
			{
				result[write++] = values1[read1];
				read1++;
			}

			Array.Resize(ref result, write);
			return result;
		}

		static bool IsOverlapping(T[] values1, T[] values2)
		{
			var read1 = 0;
			var read2 = 0;

			while (read1 < values1.Length && read2 < values2.Length)
			{
				var s1 = values1[read1];
				var s2 = values2[read2];

				var diff = s1.CompareTo(s2);

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

		static bool IsSuperSetOf(T[] values1, T[] values2)
		{
			var read1 = 0;
			var read2 = 0;

			while (read1 < values1.Length && read2 < values2.Length)
			{
				var s1 = values1[read1];
				var s2 = values2[read2];

				var diff = s1.CompareTo(s2);

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

			return read2 == values2.Length;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly T[] _values;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _cachedHashCode;
	}
}
