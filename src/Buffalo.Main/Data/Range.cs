// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using System.Globalization;

namespace Buffalo.Main
{
	[DebuggerDisplay("[{Start}, {Length}]")]
	sealed class Range : IEquatable<Range>
	{
		public Range(int start, int length)
		{
			Start = start;
			Length = length;
		}

		public int Start { get; }
		public int Length { get; }

		public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0})-({1})", Start, Start + Length - 1);
		public override bool Equals(object obj) => Equals(obj as Range);
		public bool Equals(Range other) => other != null && other.Start == Start && other.Length == Length;

		public override int GetHashCode()
		{
			unchecked
			{
				var tmp = (uint)Length;
				tmp = (tmp << 16) | (tmp >> 16);
				tmp ^= (uint)Start;
				return (int)tmp;
			}
		}
	}
}
