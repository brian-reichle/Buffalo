// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Configuration.Test
{
	[TestFixture]
	public sealed class ConfigScannerTest
	{
		[Test]
		public void Labels()
		{
			var expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.NonTerminal, new CharPos(0, 1, 1), new CharPos(12, 1, 13), \"NonTerminal\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.String, new CharPos(25, 2, 12), new CharPos(36, 2, 23), \"st\\\"ri\\\"ng\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(38, 3, 1), new CharPos(43, 3, 6), \"Label4\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(44, 3, 7), new CharPos(44, 3, 7), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("<NonTerminal>\n           \"st\\\"ri\\\"ng\"\nLabel4")), Is.EqualTo(expected));
		}

		[Test]
		public void Keywords()
		{
			var expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Null, new CharPos(0, 1, 1), new CharPos(3, 1, 4), \"null\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Entry, new CharPos(5, 1, 6), new CharPos(9, 1, 10), \"entry\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(10, 1, 11), new CharPos(10, 1, 11), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("null entry")), Is.EqualTo(expected));
		}

		[Test]
		public void Symbols()
		{
			var expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Becomes, new CharPos(0, 1, 1), new CharPos(2, 1, 3), \"::=\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Pipe, new CharPos(3, 1, 4), new CharPos(3, 1, 4), \"|\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Assign, new CharPos(4, 1, 5), new CharPos(4, 1, 5), \"=\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Semicolon, new CharPos(5, 1, 6), new CharPos(5, 1, 6), \";\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.OpenBrace, new CharPos(6, 1, 7), new CharPos(6, 1, 7), \"{\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.CloseBrace, new CharPos(7, 1, 8), new CharPos(7, 1, 8), \"}\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.OpenParen, new CharPos(8, 1, 9), new CharPos(8, 1, 9), \"(\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.CloseParen, new CharPos(9, 1, 10), new CharPos(9, 1, 10), \")\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.TargetValue, new CharPos(10, 1, 11), new CharPos(11, 1, 12), \"$$\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.ArgumentValue, new CharPos(12, 1, 13), new CharPos(13, 1, 14), \"4\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.QuestionMark, new CharPos(14, 1, 15), new CharPos(14, 1, 15), \"?\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(15, 1, 16), new CharPos(15, 1, 16), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("::=|=;{}()$$$4?")), Is.EqualTo(expected));
		}

		[Test]
		public void BrokenNonTerminal()
		{
			var expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Error, new CharPos(0, 1, 1), new CharPos(11, 1, 12), \"Unterminated non-terminal\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(13, 1, 14), new CharPos(16, 1, 17), \"snth\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(17, 1, 18), new CharPos(17, 1, 18), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("<NonTerminal snth")), Is.EqualTo(expected));
		}

		[Test]
		public void BrokenString()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Error, new CharPos(0, 1, 1), new CharPos(6, 1, 7), \"Unterminated string\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(10, 2, 2), new CharPos(13, 2, 5), \"snth\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(14, 2, 6), new CharPos(14, 2, 6), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("\"string\r\n snth")), Is.EqualTo(expected));
		}

		[Test]
		public void SingleLineComment()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(0, 1, 1), new CharPos(0, 1, 1), \"A\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(3, 2, 1), new CharPos(3, 2, 1), \"B\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(15, 3, 2), new CharPos(15, 3, 2), \"E\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(25, 4, 2), new CharPos(25, 4, 2), \"G\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(26, 4, 3), new CharPos(26, 4, 3), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("A\r\nB // C D \r\n E // F \r\n G")), Is.EqualTo(expected));
		}

		[Test]
		public void MultiLineComment()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(0, 1, 1), new CharPos(0, 1, 1), \"A\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(20, 3, 7), new CharPos(20, 3, 7), \"E\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(30, 3, 17), new CharPos(30, 3, 17), \"G\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(31, 3, 18), new CharPos(31, 3, 18), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("A /* B \r\n C \r\n D */ E /* F */ G")), Is.EqualTo(expected));
		}

		[Test]
		public void BrokenMultiLineComment()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(0, 1, 1), new CharPos(0, 1, 1), \"A\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Error, new CharPos(2, 1, 3), new CharPos(16, 3, 3), \"Unterminated comment\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(17, 3, 4), new CharPos(17, 3, 4), \"\"),\r\n" +
				"}";

			Assert.That(Renderer.Render(new ConfigScanner("A /* B \r\n C \r\n D ")), Is.EqualTo(expected));
		}
	}
}
