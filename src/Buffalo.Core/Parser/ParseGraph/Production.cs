// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser
{
	sealed class Production : IEquatable<Production>, IAppendable
	{
		public Production(Segment target, ImmutableArray<Segment> segments)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));

			_hash = CalculateHash(target, segments);
			Target = target;
			Segments = segments;
		}

		public Segment Target { get; }
		public ImmutableArray<Segment> Segments { get; }

		public bool Equals(Production other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (_hash != other._hash) return false;
			if (!Target.Equals(other.Target)) return false;

			if (Segments == other.Segments) return true;
			if (Segments.Length != other.Segments.Length) return false;

			for (var i = 0; i < Segments.Length; i++)
			{
				if (!Segments[i].Equals(other.Segments[i]))
				{
					return false;
				}
			}

			return true;
		}

		public override bool Equals(object obj) => Equals(obj as Production);
		public override int GetHashCode() => _hash;

		public override string ToString()
		{
			var builder = new StringBuilder();
			AppendTo(builder);
			return builder.ToString();
		}

		public void AppendTo(StringBuilder builder)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			builder.Append(Target.ToString());
			builder.Append(" ->");

			for (var i = 0; i < Segments.Length; i++)
			{
				builder.Append(' ');
				Segments[i].AppendTo(builder);
			}
		}

		static int CalculateHash(Segment target, ImmutableArray<Segment> segments)
		{
			var result = HashCodeBuilder.Value + target.GetHashCode();

			for (var i = 0; i < segments.Length; i++)
			{
				result += segments[i].GetHashCode();
			}

			return result;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _hash;
	}
}
