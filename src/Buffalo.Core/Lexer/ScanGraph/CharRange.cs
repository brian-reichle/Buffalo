// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Buffalo.Core.Lexer
{
	[DebuggerDisplay("{From}-{To}")]
	[StructLayout(LayoutKind.Explicit)]
	struct CharRange : IEquatable<CharRange>
	{
		[DebuggerStepThrough]
		public CharRange(char from, char to)
		{
			if (from > to) throw new ArgumentException("from cannot be after to", nameof(from));
			_hashcode = 0;
			_from = from;
			_to = to;
		}

		[DebuggerStepThrough]
		public override int GetHashCode() => _hashcode;
		public override bool Equals(object obj) => obj is CharRange && Equals((CharRange)obj);
		public bool Equals(CharRange other) => _hashcode == other._hashcode;
		public override string ToString() => string.Format(CultureInfo.InvariantCulture, "\\u{0:X4}-\\u{1:X4}", From, To);

		public static bool operator ==(CharRange range1, CharRange range2) => range1._hashcode == range2._hashcode;
		public static bool operator !=(CharRange range1, CharRange range2) => range1._hashcode != range2._hashcode;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public char From => _from;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public char To => _to;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[FieldOffset(0)]
		readonly char _from;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[FieldOffset(2)]
		readonly char _to;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[FieldOffset(0)]
		readonly int _hashcode;
	}
}
