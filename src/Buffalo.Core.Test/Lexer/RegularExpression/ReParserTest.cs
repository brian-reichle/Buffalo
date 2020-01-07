// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReParserTest
	{
		[Test]
		public void ParseSequence()
		{
			const string expected =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('C')),\r\n" +
				" new ReSingleton(CharSet.New('a')),\r\n" +
				" new ReSingleton(CharSet.New('t')),\r\n" +
				"})" +
				"";

			AssertParse("Cat", expected);
		}

		[Test]
		public void ParseSubSequence()
		{
			const string expected1 =
				"new ReEmptyString()" +
				"";

			AssertParse("()", expected1);

			const string expected2 =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('C')),\r\n" +
				" new ReUnion(new IReElement[]\r\n" +
				" {\r\n" +
				"  new ReSingleton(CharSet.New('a')),\r\n" +
				"  new ReSingleton(CharSet.New('t')),\r\n" +
				" }),\r\n" +
				"})" +
				"";

			AssertParse("C(a|t)", expected2);
		}

		[Test]
		public void ParseCharacterClass()
		{
			const string expected1 =
				"new ReSingleton(CharSet.New('a'))" +
				"";

			AssertParse("a", expected1);

			const string expected2 =
				"new ReSingleton(CharSet.New(new CharRange[]\r\n" +
				"{\r\n" +
				" new CharRange('A', 'Z'),\r\n" +
				" new CharRange('a', 'z'),\r\n" +
				"}))" +
				"";

			AssertParse("[a-zA-Z]", expected2);

			const string expected3 =
				"new ReSingleton(CharSet.New(new CharRange[]\r\n" +
				"{\r\n" +
				" new CharRange('\\0', ','),\r\n" +
				" new CharRange('.', 'W'),\r\n" +
				" new CharRange('Y', '\\\\'),\r\n" +
				" new CharRange('^', '\\uFFFF'),\r\n" +
				"}))" +
				"";

			AssertParse("[^]X-]", expected3);
		}

		[Test]
		public void ParseRepitition()
		{
			const string expected1 =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				"})" +
				"";

			AssertParse("X{3}", expected1);

			const string expected2 =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReUnion(new IReElement[]\r\n" +
				" {\r\n" +
				"  new ReSingleton(CharSet.New('X')),\r\n" +
				"  new ReEmptyString(),\r\n" +
				" }),\r\n" +
				" new ReUnion(new IReElement[]\r\n" +
				" {\r\n" +
				"  new ReSingleton(CharSet.New('X')),\r\n" +
				"  new ReEmptyString(),\r\n" +
				" }),\r\n" +
				" new ReUnion(new IReElement[]\r\n" +
				" {\r\n" +
				"  new ReSingleton(CharSet.New('X')),\r\n" +
				"  new ReEmptyString(),\r\n" +
				" }),\r\n" +
				"})" +
				"";

			AssertParse("X{,3}", expected2);

			const string expected3 =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReKleeneStar\r\n" +
				" (\r\n" +
				"  new ReSingleton(CharSet.New('X'))\r\n" +
				" ),\r\n" +
				"})" +
				"";

			AssertParse("X{3,}", expected3);

			const string expected4 =
				"new ReKleeneStar\r\n" +
				"(\r\n" +
				" new ReSingleton(CharSet.New('X'))\r\n" +
				")" +
				"";

			AssertParse("X{,}", expected4);
		}

		[Test]
		public void ParseZeroOrMore()
		{
			const string expected =
				"new ReKleeneStar\r\n" +
				"(\r\n" +
				" new ReSingleton(CharSet.New('X'))\r\n" +
				")" +
				"";

			AssertParse("X*", expected);
		}

		[Test]
		public void ParseOneOrMore()
		{
			const string expected =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReKleeneStar\r\n" +
				" (\r\n" +
				"  new ReSingleton(CharSet.New('X'))\r\n" +
				" ),\r\n" +
				"})" +
				"";

			AssertParse("X+", expected);
		}

		[Test]
		public void ParseOptional()
		{
			const string expected =
				"new ReUnion(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('X')),\r\n" +
				" new ReEmptyString(),\r\n" +
				"})" +
				"";

			AssertParse("X?", expected);
		}

		[Test]
		public void ParseChoice()
		{
			const string expected2 =
				"new ReUnion(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('A')),\r\n" +
				" new ReSingleton(CharSet.New('a')),\r\n" +
				"})" +
				"";

			AssertParse("A|a", expected2);
		}

		[Test]
		public void ParseBrokenCharClass()
		{
			AssertParseException(string.Empty, "[\\}", "unexpected end of expression");
		}

		[Test]
		public void ParseBrokenParen()
		{
			AssertParseException(string.Empty, "(a", "unexpected end of expression");
			AssertParseException(string.Empty, "(a{1}", "unexpected end of expression");
		}

		[Test]
		public void ParseBrokenRepitition()
		{
			AssertParseException(string.Empty, "a{", "unexpected end of expression");
			AssertParseException(string.Empty, "a{1", "unexpected end of expression");
			AssertParseException(string.Empty, "a{1,", "unexpected end of expression");
		}

		[Test]
		public void ParseEscapeSequenceOutsideCharClass()
		{
			const string expected =
				"new ReConcatenation(new IReElement[]\r\n" +
				"{\r\n" +
				" new ReSingleton(CharSet.New('\\0')),\r\n" +
				" new ReSingleton(CharSet.New('\\a')),\r\n" +
				" new ReSingleton(CharSet.New('\\b')),\r\n" +
				" new ReSingleton(CharSet.New('\\f')),\r\n" +
				" new ReSingleton(CharSet.New('\\n')),\r\n" +
				" new ReSingleton(CharSet.New('\\r')),\r\n" +
				" new ReSingleton(CharSet.New('\\t')),\r\n" +
				" new ReSingleton(CharSet.New('\\v')),\r\n" +
				" new ReSingleton(CharSet.New('\\u0018')),\r\n" +
				" new ReSingleton(CharSet.New('\\u001B')),\r\n" +
				"})" +
				"";

			AssertParse("\\0\\a\\b\\f\\n\\r\\t\\v\\x18\\u001B", expected);
		}

		[Test]
		public void ParseEscapeSequenceInsideCharClass()
		{
			const string expected =
				"new ReSingleton(CharSet.New(new CharRange[]\r\n" +
				"{\r\n" +
				" new CharRange('\\0', '\\0'),\r\n" +
				" new CharRange('\\a', '\\r'),\r\n" +
				" new CharRange('\\u0018', '\\u0018'),\r\n" +
				" new CharRange('\\u001B', '\\u001B'),\r\n" +
				"}))" +
				"";

			AssertParse("[\\0\\a\\b\\f\\n\\r\\t\\v\\x18\\u001B]", expected);
		}

		#region Implementation

		public static void AssertParse(string expressionString, string expectedParseTree)
		{
			AssertParse(string.Empty, expressionString, expectedParseTree);
		}

		public static void AssertParse(string message, string expressionString, string expectedParseTree)
		{
			Assert.That(Renderer.Render(ReParser.Parse(expressionString)), Is.EqualTo(expectedParseTree), message);
		}

		public static void AssertParseException(string message, string expressionString, string expectedExceptionMessage)
		{
			try
			{
				ReParser.Parse(expressionString);
				Assert.Fail(message + ": expected an exception to be thrown");
			}
			catch (ReParseException ex)
			{
				Assert.That(ex.Message, Is.EqualTo(expectedExceptionMessage), message);
			}
		}

		#endregion
	}
}
