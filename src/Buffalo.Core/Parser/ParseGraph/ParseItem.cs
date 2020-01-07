// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser
{
	sealed class ParseItem : IEquatable<ParseItem>, IAppendable
	{
		public ParseItem(Production production, int position)
		{
			if (production == null) throw new ArgumentNullException(nameof(production));

			_hash = HashCodeBuilder.Value + position.GetHashCode() + production.GetHashCode();
			_production = production;
			Position = position;
		}

		public int Position { get; }
		public Production Production => _production;

		public ParseItem NextItem()
		{
			if (Position < _production.Segments.Length)
			{
				return new ParseItem(_production, Position + 1);
			}
			else
			{
				return null;
			}
		}

		public override bool Equals(object obj) => Equals(obj as ParseItem);

		public bool Equals(ParseItem other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (other._hash != _hash) return false;
			if (other.Position != Position) return false;
			if (!other._production.Equals(_production)) return false;
			return true;
		}

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

			builder.Append("[");
			_production.Target.AppendTo(builder);
			builder.Append(" ->");

			for (var i = 0; i < Position; i++)
			{
				builder.Append(' ');
				_production.Segments[i].AppendTo(builder);
			}

			builder.Append(" \u2022");

			for (var i = Position; i < _production.Segments.Length; i++)
			{
				builder.Append(' ');
				_production.Segments[i].AppendTo(builder);
			}

			builder.Append("]");
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _hash;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Production _production;
	}
}
