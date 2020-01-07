// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Configuration.Test
{
	[TestFixture]
	public class ConfigScannerTest
	{
		[Test]
		public void Punctuation()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.OpenBrace, new CharPos(0, 1, 1), new CharPos(0, 1, 1), \"{\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.CloseBrace, new CharPos(1, 1, 2), new CharPos(1, 1, 2), \"}\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Assign, new CharPos(2, 1, 3), new CharPos(2, 1, 3), \"=\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Semicolon, new CharPos(3, 1, 4), new CharPos(3, 1, 4), \";\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(4, 1, 5), new CharPos(4, 1, 5), \"\"),\r\n" +
				"}" +
				"";

			Assert.That(Renderer.Render(new ConfigScanner("{}=;")), Is.EqualTo(expected));
		}

		[Test]
		public void LabelsAndKeywords()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.StateKeyword, new CharPos(0, 1, 1), new CharPos(4, 1, 5), \"state\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.TokenKeyword, new CharPos(7, 2, 1), new CharPos(11, 2, 5), \"token\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(14, 3, 1), new CharPos(18, 3, 5), \"label\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(19, 3, 6), new CharPos(19, 3, 6), \"\"),\r\n" +
				"}" +
				"";

			Assert.That(Renderer.Render(new ConfigScanner("state\r\ntoken\r\nlabel")), Is.EqualTo(expected));
		}

		[Test]
		public void Regex()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Regex, new CharPos(0, 1, 1), new CharPos(14, 1, 15), \"(\\\\$|[bob\\\\$])*\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Regex, new CharPos(16, 2, 1), new CharPos(17, 2, 2), \"\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Regex, new CharPos(19, 3, 1), new CharPos(31, 3, 13), \"\\\"([^\\\"]|\\\"\\\")\\\"\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(32, 3, 14), new CharPos(32, 3, 14), \"\"),\r\n" +
				"}" +
				"";

			Assert.That(Renderer.Render(new ConfigScanner("^(\\$|[bob\\$])*$\n^$\n^\"([^\"]|\"\")\"$")), Is.EqualTo(expected));
		}

		[Test]
		public void BrokenRegex()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.Error, new CharPos(0, 1, 1), new CharPos(6, 1, 7), \"Unterminated regex\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.Label, new CharPos(9, 2, 1), new CharPos(12, 2, 4), \"snth\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(13, 2, 5), new CharPos(13, 2, 5), \"\"),\r\n" +
				"}" +
				"";

			Assert.That(Renderer.Render(new ConfigScanner("^string\r\nsnth")), Is.EqualTo(expected));
		}

		[Test]
		public void String()
		{
			const string expected =
				"new ConfigToken[]\r\n" +
				"{\r\n" +
				"	new ConfigToken(ConfigTokenType.String, new CharPos(0, 1, 1), new CharPos(7, 1, 8), \"string\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.String, new CharPos(9, 1, 10), new CharPos(17, 1, 18), \"a\\\"b\\\"c\"),\r\n" +
				"	new ConfigToken(ConfigTokenType.EOF, new CharPos(18, 1, 19), new CharPos(18, 1, 19), \"\"),\r\n" +
				"}" +
				"";

			Assert.That(Renderer.Render(new ConfigScanner("\"string\" \"a\\\"b\\\"c\"")), Is.EqualTo(expected));
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
				"}" +
				"";

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
				"}" +
				"";

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
				"}" +
				"";

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
				"}" +
				"";

			Assert.That(Renderer.Render(new ConfigScanner("A /* B \r\n C \r\n D ")), Is.EqualTo(expected));
		}
	}
}
