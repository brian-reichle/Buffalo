// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Text;
using NUnit.Framework.Constraints;

namespace Buffalo.Core.Test
{
	sealed class MismatchConstraintError<T> : ConstraintError
	{
		public MismatchConstraintError(string message, T expected, T actual)
		{
			_message = message;
			_expected = expected;
			_actual = actual;
		}

		public override void WriteMessageTo(MessageWriter writer)
		{
			var builder = new StringBuilder();
			builder.AppendLine(_message);
			builder.AppendLine();
			builder.Append("Expected: ");
			builder.Append(_expected);
			builder.AppendLine();
			builder.Append("Actual:   ");
			builder.Append(_actual);
			writer.Write(builder.ToString());
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
		}

		readonly string _message;
		readonly T _expected;
		readonly T _actual;
	}
}
