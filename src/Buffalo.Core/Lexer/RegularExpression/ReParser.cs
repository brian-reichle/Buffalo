// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Buffalo.Core.Lexer
{
	static class ReParser
	{
		public static ReElement Parse(string expressionString)
		{
			using (var context = new ReParseContext(expressionString))
			{
				if (!context.MoveNext()) ThrowUnexpectedEnd();
				var result = ParseSequence(context);
				if (!context.End) ThrowUnexpectedChar(context);
				return result;
			}
		}

		static ReElement ParseSequence(ReParseContext context)
		{
			var choice = new List<ReElement>();
			var sequence = new List<ReElement>();

			var abort = false;

			while (!context.End && !abort)
			{
				switch (context.Current)
				{
					case '[':
						if (!context.MoveNext()) ThrowUnexpectedEnd();
						sequence.Add(ParseCharSet(context));
						if (context.End) ThrowUnexpectedEnd();
						if (context.Current != ']') ThrowUnexpectedChar(context);
						context.MoveNext();
						break;

					case '(':
						if (!context.MoveNext()) ThrowUnexpectedEnd();
						sequence.Add(ParseSequence(context));
						if (context.End) ThrowUnexpectedEnd();
						if (context.Current != ')') ThrowUnexpectedChar(context);
						context.MoveNext();
						break;

					case ')':
						abort = true;
						break;

					case '|':
						choice.Add(ReFactory.NewConcatenation(sequence));
						sequence.Clear();
						context.MoveNext();
						break;

					case '.':
						sequence.Add(ReFactory.NewSingleton(CharSet.LineBody));
						context.MoveNext();
						break;

					case '?':
						if (sequence.Count == 0) ThrowUnexpectedChar(context);
						sequence[sequence.Count - 1] = ReFactory.NewRepetition(sequence[sequence.Count - 1], 0, 1);
						context.MoveNext();
						break;

					case '+':
						if (sequence.Count == 0) ThrowUnexpectedChar(context);
						sequence[sequence.Count - 1] = ReFactory.NewRepetition(sequence[sequence.Count - 1], 1, null);
						context.MoveNext();
						break;

					case '*':
						if (sequence.Count == 0) ThrowUnexpectedChar(context);
						sequence[sequence.Count - 1] = ReFactory.NewRepetition(sequence[sequence.Count - 1], 0, null);
						context.MoveNext();
						break;

					case '{':
						if (sequence.Count == 0) ThrowUnexpectedChar(context);
						if (!context.MoveNext()) ThrowUnexpectedEnd();
						sequence[sequence.Count - 1] = ParseRepitition(sequence[sequence.Count - 1], context);
						if (context.Current != '}') ThrowUnexpectedChar(context);
						context.MoveNext();
						break;

					case '\\':
						if (!context.MoveNext()) ThrowUnexpectedEnd();
						sequence.Add(ReFactory.NewSingleton(CharSet.New(ParseEscapeSequence(context))));
						break;

					default:
						sequence.Add(ReFactory.NewSingleton(CharSet.New(context.Current)));
						context.MoveNext();
						break;
				}
			}

			choice.Add(ReFactory.NewConcatenation(sequence));
			return ReFactory.NewUnion(choice);
		}

		static ReElement ParseCharSet(ReParseContext context)
		{
			var ranges = new List<CharRange>();
			var invert = false;

			if (context.Current == '^')
			{
				invert = true;
				if (!context.MoveNext()) ThrowUnexpectedEnd();
			}

			if (context.Current == ']')
			{
				ranges.Add(new CharRange(']', ']'));
				if (!context.MoveNext()) ThrowUnexpectedEnd();
			}

			var abort = false;

			while (!context.End && !abort)
			{
				switch (context.Current)
				{
					case ']':
						abort = true;
						break;

					case '-':
						if (ranges.Count == 0)
						{
							ranges.Add(new CharRange('-', '-'));
						}
						else
						{
							var from = ranges[ranges.Count - 1].To;
							if (!context.MoveNext()) ThrowUnexpectedEnd();
							var to = context.Current;

							if (to == ']')
							{
								ranges.Add(new CharRange('-', '-'));
							}
							else
							{
								if (!context.MoveNext()) ThrowUnexpectedEnd();

								if (to == '\\')
								{
									to = ParseEscapeSequence(context);
									if (context.End) ThrowUnexpectedEnd();
								}

								if (to < from) ThrowBrokenCharRange(from, to);

								ranges.Add(new CharRange(from, to));
							}
						}

						break;

					case '\\':
						if (!context.MoveNext()) ThrowUnexpectedEnd();
						var c = ParseEscapeSequence(context);
						ranges.Add(new CharRange(c, c));
						break;

					default:
						ranges.Add(new CharRange(context.Current, context.Current));
						if (!context.MoveNext()) ThrowUnexpectedEnd();
						break;
				}
			}

			var set = CharSet.New(ranges.ToArray());

			if (invert)
			{
				set = CharSet.Universal.Subtract(set);
			}

			return ReFactory.NewSingleton(set);
		}

		static ReElement ParseRepitition(ReElement element, ReParseContext context)
		{
			var min = ParseInt(context) ?? 0;

			if (context.Current == ',')
			{
				if (!context.MoveNext()) ThrowUnexpectedEnd();
				var max = ParseInt(context);
				return ReFactory.NewRepetition(element, min, max);
			}
			else
			{
				return ReFactory.NewRepetition(element, min, min);
			}
		}

		static char ParseEscapeSequence(ReParseContext context)
		{
			var c = context.Current;
			context.MoveNext();

			switch (c)
			{
				case '0': return '\0';
				case 'a': return '\a';
				case 'b': return '\b';
				case 'f': return '\f';
				case 'n': return '\n';
				case 'r': return '\r';
				case 't': return '\t';
				case 'v': return '\v';

				case 'x': return (char)ParseHex(context, 2);
				case 'u': return (char)ParseHex(context, 4);

				default: return c;
			}
		}

		static int? ParseInt(ReParseContext context)
		{
			var builder = new StringBuilder();
			var startIndex = context.Position;

			while (context.Current >= '0' && context.Current <= '9')
			{
				builder.Append(context.Current);
				if (!context.MoveNext()) ThrowUnexpectedEnd();
			}

			if (builder.Length == 0)
			{
				return null;
			}
			else if (int.TryParse(builder.ToString(), out var result))
			{
				return result;
			}
			else
			{
				throw new ReParseException(string.Format(CultureInfo.InvariantCulture, "error parsing int at {0}", startIndex));
			}
		}

		static int ParseHex(ReParseContext context, int count)
		{
			var total = 0;

			for (var i = 0; i < count; i++)
			{
				if (context.End) ThrowUnexpectedEnd();

				var c = context.Current;
				int nibble;

				if (c >= '0' && c <= '9')
				{
					nibble = c - '0';
				}
				else if (c >= 'a' && c <= 'f')
				{
					nibble = c - 'a' + 10;
				}
				else if (c >= 'A' && c <= 'F')
				{
					nibble = c - 'A' + 10;
				}
				else
				{
					ThrowUnexpectedChar(context);
					throw new InvalidOperationException("ThrowUnexpectedChar should have thrown an exception");
				}

				total = (total << 4) | nibble;

				context.MoveNext();
			}

			return total;
		}

		static void ThrowUnexpectedEnd()
			=> throw new ReParseException("unexpected end of expression");

		static void ThrowUnexpectedChar(ReParseContext context)
			=> throw new ReParseException(string.Format(CultureInfo.CurrentCulture, "unexpected char '{0}' at position {1}", context.Current, context.Position));

		static void ThrowBrokenCharRange(char pre, char post)
			=> throw new ReParseException(string.Format(CultureInfo.CurrentCulture, "broken char range '{0}' - '{1}'", pre, post));
	}
}
