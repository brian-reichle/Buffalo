// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser
{
	[DebuggerDisplay("Name = {Name}, Is Terminal = {IsTerminal}")]
	sealed class Segment : IComparable, IComparable<Segment>, IEquatable<Segment>, IAppendable
	{
		public Segment(string name, bool isTerminal)
			: this(name, isTerminal ? SegmentFlags.IsTerminal : SegmentFlags.None)
		{
		}

		Segment(string name, SegmentFlags flags)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			Name = name;
			_flags = flags;
		}

		public static Segment EOF { get; } = new Segment("EOF", true);
		public static Segment Error { get; } = new Segment("Error", true);

		public string Name { get; }
		public bool IsTerminal => (_flags & SegmentFlags.IsTerminal) != 0;
		public bool IsInitial => (_flags & SegmentFlags.IsInitial) != 0;

		public Segment GetOptional()
		{
			switch (_flags)
			{
				case SegmentFlags.None:
					return new Segment(Name, SegmentFlags.Optional);

				case SegmentFlags.IsTerminal:
					return new Segment(Name, SegmentFlags.IsBaseTerminal | SegmentFlags.Optional);

				default:
					throw new InvalidOperationException("Modifier cannot be applied");
			}
		}

		public Segment GetInitial()
		{
			switch (_flags)
			{
				case SegmentFlags.None:
					return new Segment(Name, SegmentFlags.IsInitial);

				default:
					throw new InvalidOperationException("Modifier cannot be applied");
			}
		}

		public override int GetHashCode() => HashCodeBuilder.Value + Name.GetHashCode() + _flags.GetHashCode();

		public override bool Equals(object obj) => Equals(obj as Segment);
		public bool Equals(Segment other) => other != null && _flags == other._flags && Name == other.Name;

		public override string ToString()
		{
			var builder = new StringBuilder();
			AppendTo(builder);
			return builder.ToString();
		}

		public void AppendTo(StringBuilder builder)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			const SegmentFlags terminalMask = SegmentFlags.IsTerminal | SegmentFlags.IsBaseTerminal;

			if ((_flags & terminalMask) != 0)
			{
				builder.Append(Name);
			}
			else if ((_flags & SegmentFlags.IsInitial) != 0)
			{
				builder.Append('<');
				builder.Append(Name);
				builder.Append("'>");
			}
			else
			{
				builder.Append('<');
				builder.Append(Name);
				builder.Append('>');
			}

			if ((_flags & SegmentFlags.Optional) != 0)
			{
				builder.Append('?');
			}
		}

		public int CompareTo(Segment other)
		{
			int diff;

			diff = other._flags.CompareTo(_flags);
			if (diff != 0)
			{
				return diff;
			}

			diff = string.CompareOrdinal(Name, other.Name);
			return diff;
		}

		int IComparable.CompareTo(object obj) => CompareTo((Segment)obj);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly SegmentFlags _flags;
	}
}
