// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Lexer
{
	sealed class CharSet : IEnumerable<CharRange>, IAppendable
	{
		static CharSet()
		{
			Universal = new CharSet(new CharRange[] { new CharRange(char.MinValue, char.MaxValue) });

			LineBody = new CharSet(SubtractRanges(Universal._ranges, new CharRange[]
			{
				new CharRange('\n', '\n'),
				new CharRange('\r', '\r'),
			}));
		}

		CharSet(CharRange[] ranges)
		{
			_ranges = ranges;
		}

		public static CharSet Universal { get; }
		public static CharSet LineBody { get; }
		public bool IsEmptySet => _ranges.Length == 0;

		public static CharSet New(CharRange[] ranges)
		{
			if (ranges == null) throw new ArgumentNullException(nameof(ranges));
			return new CharSet(SanitiseRanges(ranges));
		}

		public static CharSet New(char character)
		{
			return new CharSet(new CharRange[] { new CharRange(character, character) });
		}

		public override string ToString()
		{
			var builder = new StringBuilder((_ranges.Length * 4) + 3);
			AppendTo(builder);
			return builder.ToString();
		}

		public CharSet Union(CharSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));
			return Select(otherSet, UnionRanges(_ranges, otherSet._ranges));
		}

		public CharSet Intersection(CharSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));
			return Select(otherSet, IntersectRanges(_ranges, otherSet._ranges));
		}

		public CharSet Subtract(CharSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));
			return Select(otherSet, SubtractRanges(_ranges, otherSet._ranges));
		}

		public bool Intersects(CharSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));
			return IsOverlappingRanges(_ranges, otherSet._ranges);
		}

		public bool IsSupersetOf(CharSet otherSet)
		{
			if (otherSet == null) throw new ArgumentNullException(nameof(otherSet));
			return IsSuperSetOfRanges(_ranges, otherSet._ranges);
		}

		public bool ContainsChar(char character)
		{
			var lower = 0;
			var upper = _ranges.Length - 1;

			while (lower <= upper)
			{
				var mid = (upper + lower) >> 1;
				var r = _ranges[mid];

				if (character < r.From)
				{
					upper = mid - 1;
				}
				else if (character > r.To)
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

		public IEnumerator<CharRange> GetEnumerator() => ((IEnumerable<CharRange>)_ranges).GetEnumerator();

		public void AppendTo(StringBuilder builder)
		{
			var invert = _ranges.Length > 0 && _ranges[_ranges.Length - 1].To == '\uffff';

			if (invert)
			{
				builder.Append('!');
			}

			builder.Append('[');
			AppendRanges(builder, invert ? SubtractRanges(Universal._ranges, _ranges) : _ranges);
			builder.Append(']');
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		CharSet Select(CharSet otherSet, CharRange[] ranges)
		{
			if (_ranges == ranges)
			{
				return this;
			}
			else if (otherSet._ranges == ranges)
			{
				return otherSet;
			}
			else
			{
				return new CharSet(ranges);
			}
		}

		static void AppendRanges(StringBuilder builder, CharRange[] ranges)
		{
			if (ranges.Length > 0)
			{
				var r = ranges[0];
				CharEscapeHelper.AppendEscapedChar(builder, r.From);

				if (r.To != r.From)
				{
					builder.Append('-');
					CharEscapeHelper.AppendEscapedChar(builder, r.To);
				}

				for (var i = 1; i < ranges.Length; i++)
				{
					r = ranges[i];
					builder.Append(',');
					CharEscapeHelper.AppendEscapedChar(builder, r.From);

					if (r.To != r.From)
					{
						builder.Append('-');
						CharEscapeHelper.AppendEscapedChar(builder, r.To);
					}
				}
			}
		}

		static CharRange[] SanitiseRanges(CharRange[] ranges)
		{
			if (ranges.Length < 1)
			{
				return ranges;
			}
			else
			{
				var result = new CharRange[ranges.Length];
				Array.Copy(ranges, result, result.Length);

				if (result.Length > 1)
				{
					Array.Sort(result, (r1, r2) => r1.From - r2.From);

					var lastRange = result[0];

					var write = 0;

					for (var read = 1; read < result.Length; read++)
					{
						var nextRange = result[read];

						if (lastRange.To + 1 < nextRange.From)
						{
							result[write++] = lastRange;
							lastRange = nextRange;
						}
						else if (nextRange.To > lastRange.To)
						{
							lastRange = new CharRange(lastRange.From, nextRange.To);
						}
					}

					result[write++] = lastRange;
					Array.Resize(ref result, write);
				}

				return result;
			}
		}

		static CharRange[] UnionRanges(CharRange[] ranges1, CharRange[] ranges2)
		{
			if (ranges1.Length == 0)
			{
				return ranges2;
			}
			else if (ranges2.Length == 0)
			{
				return ranges1;
			}
			else
			{
				var result = new CharRange[ranges1.Length + ranges2.Length];

				var read1 = 0;
				var read2 = 0;
				var write = 0;
				CharRange lastRange;

				if (ranges1[read1].From < ranges2[read2].From)
				{
					lastRange = ranges1[read1++];
				}
				else
				{
					lastRange = ranges2[read2++];
				}

				while (read1 < ranges1.Length && read2 < ranges2.Length)
				{
					CharRange nextRange;
					var r1 = ranges1[read1];
					var r2 = ranges2[read2];

					if (r1.From < r2.From)
					{
						nextRange = r1;
						read1++;
					}
					else
					{
						nextRange = r2;
						read2++;
					}

					if (lastRange.To + 1 < nextRange.From)
					{
						result[write++] = lastRange;
						lastRange = nextRange;
					}
					else if (nextRange.To > lastRange.To)
					{
						lastRange = new CharRange(lastRange.From, nextRange.To);
					}
				}

				while (read1 < ranges1.Length)
				{
					var nextRange = ranges1[read1++];

					if (lastRange.To + 1 < nextRange.From)
					{
						result[write++] = lastRange;
						lastRange = nextRange;
					}
					else if (nextRange.To > lastRange.To)
					{
						lastRange = new CharRange(lastRange.From, nextRange.To);
					}
				}

				while (read2 < ranges2.Length)
				{
					var nextRange = ranges2[read2++];

					if (lastRange.To + 1 < nextRange.From)
					{
						result[write++] = lastRange;
						lastRange = nextRange;
					}
					else if (nextRange.To > lastRange.To)
					{
						lastRange = new CharRange(lastRange.From, nextRange.To);
					}
				}

				result[write++] = lastRange;

				Array.Resize(ref result, write);
				return result;
			}
		}

		static CharRange[] IntersectRanges(CharRange[] ranges1, CharRange[] ranges2)
		{
			if (ranges1.Length == 0)
			{
				return ranges1;
			}
			else if (ranges2.Length == 0)
			{
				return ranges2;
			}
			else
			{
				var result = new CharRange[ranges1.Length + ranges2.Length - 1];

				var read1 = 0;
				var read2 = 0;
				var write = 0;

				while (read1 < ranges1.Length && read2 < ranges2.Length)
				{
					var r1 = ranges1[read1];
					var r2 = ranges2[read2];

					if (r1.To < r2.From)
					{
						read1++;
					}
					else if (r1.From > r2.To)
					{
						read2++;
					}
					else
					{
						char from;
						char to;

						if (r1.To < r2.To)
						{
							read1++;
							to = r1.To;
						}
						else if (r1.To > r2.To)
						{
							read2++;
							to = r2.To;
						}
						else
						{
							read1++;
							read2++;
							to = r1.To;
						}

						if (r1.From > r2.From)
						{
							from = r1.From;
						}
						else
						{
							from = r2.From;
						}

						result[write++] = new CharRange(from, to);
					}
				}

				Array.Resize(ref result, write);
				return result;
			}
		}

		static CharRange[] SubtractRanges(CharRange[] ranges1, CharRange[] ranges2)
		{
			if (ranges1.Length == 0 || ranges2.Length == 0)
			{
				return ranges1;
			}
			else
			{
				var result = new CharRange[ranges1.Length + ranges2.Length];

				var read1 = 0;
				var read2 = 0;
				var write = 0;

				var current = new CharRange();
				var currentDirty = true;

				while (read1 < ranges1.Length && read2 < ranges2.Length)
				{
					if (currentDirty)
					{
						current = ranges1[read1];
						currentDirty = false;
					}

					var r2 = ranges2[read2];

					if (current.From > r2.To)
					{
						read2++;
					}
					else if (current.To < r2.From)
					{
						result[write++] = current;
						read1++;
						currentDirty = true;
					}
					else
					{
						if (r2.From > current.From)
						{
							result[write++] = new CharRange(current.From, (char)(r2.From - 1));
						}

						if (r2.To < current.To)
						{
							current = new CharRange((char)(r2.To + 1), current.To);
							read2++;
						}
						else
						{
							currentDirty = true;
							read1++;
						}
					}
				}

				if (!currentDirty)
				{
					result[write++] = current;
					read1++;
				}

				while (read1 < ranges1.Length)
				{
					result[write++] = ranges1[read1++];
				}

				Array.Resize(ref result, write);
				return result;
			}
		}

		static bool IsOverlappingRanges(CharRange[] ranges1, CharRange[] ranges2)
		{
			if (ranges1.Length == 0 || ranges2.Length == 0)
			{
				return false;
			}
			else
			{
				var read1 = 0;
				var read2 = 0;

				while (read1 < ranges1.Length && read2 < ranges2.Length)
				{
					var r1 = ranges1[read1];
					var r2 = ranges2[read2];

					if (r1.To < r2.From)
					{
						read1++;
					}
					else if (r2.To < r1.From)
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
		}

		static bool IsSuperSetOfRanges(CharRange[] ranges1, CharRange[] ranges2)
		{
			if (ranges2.Length == 0)
			{
				return true;
			}
			else if (ranges1.Length == 0)
			{
				return false;
			}
			else
			{
				var read1 = 0;
				var read2 = 0;

				while (read1 < ranges1.Length && read2 < ranges2.Length)
				{
					var r1 = ranges1[read1];
					var r2 = ranges2[read2];

					if (r1.To < r2.From)
					{
						read1++;
					}
					else if (r1.From > r2.From)
					{
						return false;
					}
					else if (r1.To < r2.To)
					{
						return false;
					}
					else
					{
						read2++;
					}
				}

				return read2 == ranges2.Length;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly CharRange[] _ranges;
	}
}
