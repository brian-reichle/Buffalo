// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace Buffalo.Core.Lexer
{
	[Serializable]
	public sealed class ReParseException : Exception
	{
		public ReParseException()
		{
		}

		public ReParseException(string message)
			: base(message)
		{
		}

		ReParseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public ReParseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
