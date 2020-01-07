// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Buffalo.Core.Lexer
{
	sealed class ReParseContext : IEnumerator<char>
	{
		public ReParseContext(string expressionString)
		{
			ExpressionString = expressionString;
			Position = -1;
		}

		public string ExpressionString { get; }
		public int Position { get; private set; }
		public bool End => Position >= ExpressionString.Length;
		public char Current => ExpressionString[Position];
		public bool MoveNext() => (++Position) < ExpressionString.Length;
		public void Reset() => Position = -1;

		object IEnumerator.Current => Current;

		void IDisposable.Dispose()
		{
		}
	}
}
