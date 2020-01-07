// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture(typeof(Generated.CacheMultiEntry.AutoScanner<Token>), "LexerCacheMultiEntry")]
	[TestFixture(typeof(Generated.CacheMultiTable.AutoScanner<Token>), "LexerCacheMultiTable")]
	[TestFixture(typeof(Generated.MultiEntry.AutoScanner<Token>), "LexerMultiEntry")]
	[TestFixture(typeof(Generated.MultiTable.AutoScanner<Token>), "LexerMultiTable")]
	public sealed class CommonMultiLexerTest<T>
	{
		public CommonMultiLexerTest(string setName)
		{
			_setName = setName;
		}

		[Test]
		public void Generate()
		{
			CommonLexerTest.AssertGeneration(LexerTestFiles.GetNamedResourceSet(_setName), true);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Scan(bool preScan)
		{
			const string Source = @"
4 * (A <% cat %> + 7)
";

			const string Expected =
@"new Token[]
{
	new Token(""Number"", ""4"", new CharPos(2, 2, 1), new CharPos(2, 2, 1)),
	new Token(""Multiply"", ""*"", new CharPos(4, 2, 3), new CharPos(4, 2, 3)),
	new Token(""OpenParen"", ""("", new CharPos(6, 2, 5), new CharPos(6, 2, 5)),
	new Token(""Label"", ""A"", new CharPos(7, 2, 6), new CharPos(7, 2, 6)),
	new Token(""OtherLabel"", ""cat"", new CharPos(12, 2, 11), new CharPos(14, 2, 13)),
	new Token(""Plus"", ""+"", new CharPos(19, 2, 18), new CharPos(19, 2, 18)),
	new Token(""Number"", ""7"", new CharPos(21, 2, 20), new CharPos(21, 2, 20)),
	new Token(""CloseParen"", "")"", new CharPos(22, 2, 21), new CharPos(22, 2, 21)),
	new Token(""EOF"", """", new CharPos(25, 3, 1), new CharPos(25, 3, 1)),
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
		public void EOFInOtherState()
		{
			const string Source = @"
4 * (A <% cat
";

			const string Expected =
@"new Token[]
{
	new Token(""Number"", ""4"", new CharPos(2, 2, 1), new CharPos(2, 2, 1)),
	new Token(""Multiply"", ""*"", new CharPos(4, 2, 3), new CharPos(4, 2, 3)),
	new Token(""OpenParen"", ""("", new CharPos(6, 2, 5), new CharPos(6, 2, 5)),
	new Token(""Label"", ""A"", new CharPos(7, 2, 6), new CharPos(7, 2, 6)),
	new Token(""OtherLabel"", ""cat"", new CharPos(12, 2, 11), new CharPos(14, 2, 13)),
	new Token(""Error"", ""Unexpected EOF"", new CharPos(17, 3, 1), new CharPos(17, 3, 1)),
	new Token(""EOF"", """", new CharPos(17, 3, 1), new CharPos(17, 3, 1)),
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
					case "Whitespace2":
						return null;

					case "EndOther":
						s = null;
						return null;

					case "StartOther":
						s = "Other";
						return null;

					case "EOF":
						if (s == "Other")
						{
							return new Token("Error", "Unexpected EOF", token.FromPos, token.ToPos);
						}

						return token;

					case "Label":
						if (s == "Other")
						{
							goto case "Label2";
						}

						return token;

					case "Label2": return new Token("OtherLabel", token.Value, token.FromPos, token.ToPos);

					default: return token;
				}
			});
		}

		readonly string _setName;

		#endregion
	}
}
