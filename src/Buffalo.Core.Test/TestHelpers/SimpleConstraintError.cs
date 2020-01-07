// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Diagnostics;
using NUnit.Framework.Constraints;

namespace Buffalo.Core.Test
{
	[DebuggerDisplay("{_message}")]
	sealed class SimpleConstraintError : ConstraintError
	{
		public SimpleConstraintError(string message)
			: this(message, null)
		{
		}

		public SimpleConstraintError(string message, string description)
		{
			_message = message;
			_description = description;
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
			if (_description != null)
			{
				writer.Write(_description);
			}
		}

		public override void WriteMessageTo(MessageWriter writer)
		{
			writer.Write(_message);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly string _message;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly string _description;
	}
}
