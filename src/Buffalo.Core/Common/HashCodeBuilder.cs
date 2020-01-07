// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Globalization;

namespace Buffalo.Core.Common
{
	readonly struct HashCodeBuilder : IEquatable<HashCodeBuilder>
	{
		public static HashCodeBuilder Value => new HashCodeBuilder(17);

		public HashCodeBuilder(uint value)
		{
			_value = value;
		}

		public HashCodeBuilder(int value)
			: this(unchecked((uint)value))
		{
		}

		public override int GetHashCode() => unchecked((int)_value);
		public override string ToString() => _value.ToString("X8", CultureInfo.InvariantCulture);
		public bool Equals(HashCodeBuilder other) => other._value == _value;
		public override bool Equals(object obj) => obj is HashCodeBuilder && Equals((HashCodeBuilder)obj);

		public HashCodeBuilder Add(uint value) => new HashCodeBuilder((_value * 23) + value);
		public HashCodeBuilder Add(int value) => Add(unchecked((uint)value));
		public HashCodeBuilder Add(bool value) => Add(value ? 1 : 0);
		public HashCodeBuilder Add(int? value) => Add(value.HasValue).Add(value.GetValueOrDefault());

		public static implicit operator HashCodeBuilder(int value) => new HashCodeBuilder(value);
		public static implicit operator int(HashCodeBuilder value) => unchecked((int)value._value);

		public static HashCodeBuilder operator +(HashCodeBuilder x, uint y) => x.Add(y);
		public static HashCodeBuilder operator +(HashCodeBuilder x, int y) => x.Add(y);
		public static HashCodeBuilder operator +(HashCodeBuilder x, bool y) => x.Add(y);
		public static HashCodeBuilder operator +(HashCodeBuilder x, int? y) => x.Add(y);

		readonly uint _value;
	}
}
