// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigScanner : AutoConfigScanner<ConfigToken>
	{
		public ConfigScanner(string expressionString)
			: base(expressionString)
		{
		}

		protected override ConfigToken NewToken(AutoConfigScanner<ConfigToken>.TokenType type, string expressionString, int startPosition, int length)
		{
			switch (type)
			{
				case AutoConfigScanner<ConfigToken>.TokenType.ArgumentValue:
					return NewFixedToken(ConfigTokenType.ArgumentValue, startPosition, length, expressionString.Substring(startPosition + 1, length - 1));

				case AutoConfigScanner<ConfigToken>.TokenType.Assign:
					return NewFixedToken(ConfigTokenType.Assign, startPosition, length, "=");

				case AutoConfigScanner<ConfigToken>.TokenType.Bang:
					return NewFixedToken(ConfigTokenType.Bang, startPosition, length, "!");

				case AutoConfigScanner<ConfigToken>.TokenType.Becomes:
					return NewFixedToken(ConfigTokenType.Becomes, startPosition, length, "::=");

				case AutoConfigScanner<ConfigToken>.TokenType.BrokenComment:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unterminated comment");

				case AutoConfigScanner<ConfigToken>.TokenType.BrokenNonTerminal:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unterminated non-terminal");

				case AutoConfigScanner<ConfigToken>.TokenType.BrokenString:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unterminated string");

				case AutoConfigScanner<ConfigToken>.TokenType.CloseBrace:
					return NewFixedToken(ConfigTokenType.CloseBrace, startPosition, length, "}");

				case AutoConfigScanner<ConfigToken>.TokenType.CloseParen:
					return NewFixedToken(ConfigTokenType.CloseParen, startPosition, length, ")");

				case AutoConfigScanner<ConfigToken>.TokenType.Entry:
					return NewFixedToken(ConfigTokenType.Entry, startPosition, length, "entry");

				case AutoConfigScanner<ConfigToken>.TokenType.EOF:
					return NewFixedToken(ConfigTokenType.EOF, startPosition, length, string.Empty);

				case AutoConfigScanner<ConfigToken>.TokenType.Label:
					return NewFixedToken(ConfigTokenType.Label, startPosition, length, expressionString.Substring(startPosition, length));

				case AutoConfigScanner<ConfigToken>.TokenType.NonTerminal:
					return NewFixedToken(ConfigTokenType.NonTerminal, startPosition, length, expressionString.Substring(startPosition + 1, length - 2));

				case AutoConfigScanner<ConfigToken>.TokenType.Null:
					return NewFixedToken(ConfigTokenType.Null, startPosition, length, "null");

				case AutoConfigScanner<ConfigToken>.TokenType.OpenBrace:
					return NewFixedToken(ConfigTokenType.OpenBrace, startPosition, length, "{");

				case AutoConfigScanner<ConfigToken>.TokenType.OpenParen:
					return NewFixedToken(ConfigTokenType.OpenParen, startPosition, length, "(");

				case AutoConfigScanner<ConfigToken>.TokenType.Pipe:
					return NewFixedToken(ConfigTokenType.Pipe, startPosition, length, "|");

				case AutoConfigScanner<ConfigToken>.TokenType.QuestionMark:
					return NewFixedToken(ConfigTokenType.QuestionMark, startPosition, length, "?");

				case AutoConfigScanner<ConfigToken>.TokenType.Semicolon:
					return NewFixedToken(ConfigTokenType.Semicolon, startPosition, length, ";");

				case AutoConfigScanner<ConfigToken>.TokenType.String:
					{
						var builder = new StringBuilder(length - 2);

						var endPosition = startPosition + length - 2;
						for (var i = startPosition + 1; i <= endPosition; i++)
						{
							if (expressionString[i] == '\\')
							{
								i++;
							}

							builder.Append(expressionString[i]);
						}

						return NewFixedToken(ConfigTokenType.String, startPosition, length, builder.ToString());
					}

				case AutoConfigScanner<ConfigToken>.TokenType.TargetValue:
					return NewFixedToken(ConfigTokenType.TargetValue, startPosition, length, "$$");

				case AutoConfigScanner<ConfigToken>.TokenType.Using:
					return NewFixedToken(ConfigTokenType.Using, startPosition, length, "using");

				case AutoConfigScanner<ConfigToken>.TokenType.Comment:
				case AutoConfigScanner<ConfigToken>.TokenType.Whitespace:
					return null;

				default:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unknown token type");
			}
		}

		ConfigToken NewFixedToken(ConfigTokenType type, int startPosition, int length, string text)
		{
			var fromPos = NewCharPos(startPosition);
			var toPos = length > 1 ? NewCharPos(startPosition + length - 1) : fromPos;

			return new ConfigToken(type, fromPos, toPos, text);
		}

		CharPos NewCharPos(int index)
		{
			StartOfLine(index, out var lineNo, out var charNo);

			return new CharPos(index, lineNo, charNo);
		}
	}
}
