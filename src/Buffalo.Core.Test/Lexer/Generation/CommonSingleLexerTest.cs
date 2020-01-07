// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture(typeof(Generated.Base.AutoScanner<Token>), "LexerBase")]
	[TestFixture(typeof(Generated.Byte.AutoScanner<Token>), "LexerByte")]
	[TestFixture(typeof(Generated.Int.AutoScanner<Token>), "LexerInt")]
	[TestFixture(typeof(Generated.Short.AutoScanner<Token>), "LexerShort")]
	[TestFixture(typeof(Generated.CacheTables.AutoScanner<Token>), "LexerCacheTables")]
	[TestFixture(typeof(Generated.SuppressEmbedding.AutoScanner<Token>), "LexerSuppressEmbedding", false)]
	[TestFixture(typeof(Generated.CTB.AutoScanner<Token>), "LexerCTB")]
	[TestFixture(typeof(Generated.None.AutoScanner<Token>), "LexerNone")]
	[TestFixture(typeof(Generated.Simple.AutoScanner<Token>), "LexerSimple")]
	[TestFixture(typeof(Generated.Internal.AutoScanner<Token>), "LexerInternal")]
	[TestFixture(typeof(Generated.Public.AutoScanner<Token>), "LexerPublic")]
	public sealed class CommonSingleLexerTest<T>
	{
		public CommonSingleLexerTest(string setName)
			: this(setName, true)
		{
		}

		public CommonSingleLexerTest(string setName, bool embedTable)
		{
			_setName = setName;
			_embedTable = embedTable;
		}

		[Test]
		public void Generate()
		{
			CommonLexerTest.AssertGeneration(LexerTestFiles.GetNamedResourceSet(_setName), _embedTable);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Scan(bool preScan)
		{
			const string Source = @"
4 * (A + 7)
";

			const string Expected =
@"new Token[]
{
	new Token(""Number"", ""4"", new CharPos(2, 2, 1), new CharPos(2, 2, 1)),
	new Token(""Multiply"", ""*"", new CharPos(4, 2, 3), new CharPos(4, 2, 3)),
	new Token(""OpenParen"", ""("", new CharPos(6, 2, 5), new CharPos(6, 2, 5)),
	new Token(""Label"", ""A"", new CharPos(7, 2, 6), new CharPos(7, 2, 6)),
	new Token(""Plus"", ""+"", new CharPos(9, 2, 8), new CharPos(9, 2, 8)),
	new Token(""Number"", ""7"", new CharPos(11, 2, 10), new CharPos(11, 2, 10)),
	new Token(""CloseParen"", "")"", new CharPos(12, 2, 11), new CharPos(12, 2, 11)),
	new Token(""EOF"", """", new CharPos(15, 3, 1), new CharPos(15, 3, 1)),
}
";

			var result = NewLexer(Source);

			if (preScan)
			{
				result.Count();
			}

			Assert.That(Renderer.Render(result), Is.EqualTo(Expected));
		}

		[Test]
		public void Override()
		{
			const string Source = @"
cat
dog
other
";

			const string Expected =
@"new Token[]
{
	new Token(""Cat"", ""cat"", new CharPos(2, 2, 1), new CharPos(4, 2, 3)),
	new Token(""Dog"", ""dog"", new CharPos(7, 3, 1), new CharPos(9, 3, 3)),
	new Token(""Label"", ""other"", new CharPos(12, 4, 1), new CharPos(16, 4, 5)),
	new Token(""EOF"", """", new CharPos(19, 5, 1), new CharPos(19, 5, 1)),
}
";

			var result = NewLexer(Source);
			Assert.That(Renderer.Render(result), Is.EqualTo(Expected));
		}

		[Test]
		public void UnmatchedChar()
		{
			Assert.That(
				() =>
				{
					NewLexer("cat ; dog").Count();
				},
				Throws.InvalidOperationException.With.Message.EqualTo("Got stuck at position 4."));
		}

		#region Implementation

		IEnumerable<Token> NewLexer(string expressionString)
		{
			var baseType = typeof(T);

			if (baseType.IsGenericType)
			{
				baseType = baseType.GetGenericTypeDefinition();
			}

			return LexerFactory.NewLexer(baseType, expressionString, delegate (ref string s, Token token)
			{
				switch (token.TokenTypeName)
				{
					case "Whitespace":
						return null;

					default: return token;
				}
			});
		}

		readonly string _setName;
		readonly bool _embedTable;

		#endregion
	}
}
