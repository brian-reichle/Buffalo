// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Core
{
	public static class GeneratorFactory
	{
		public static ICodeGenerator NewGenerator(GeneratorType type)
		{
			switch (type)
			{
				case GeneratorType.Lexer: return new Lexer.LexerGenerator();
				case GeneratorType.Parser: return new Parser.ParserGenerator();
				default: throw new ArgumentException("Unknown generator type", nameof(type));
			}
		}
	}
}
