// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Lexer.Configuration
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
				case AutoConfigScanner<ConfigToken>.TokenType.Assign:
					return NewFixedToken(ConfigTokenType.Assign, startPosition, length, "=");

				case AutoConfigScanner<ConfigToken>.TokenType.BrokenComment:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unterminated comment");

				case AutoConfigScanner<ConfigToken>.TokenType.BrokenRegex:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unterminated regex");

				case AutoConfigScanner<ConfigToken>.TokenType.BrokenString:
					return NewFixedToken(ConfigTokenType.Error, startPosition, length, "Unterminated string");

				case AutoConfigScanner<ConfigToken>.TokenType.CloseBrace:
					return NewFixedToken(ConfigTokenType.CloseBrace, startPosition, length, "}");

				case AutoConfigScanner<ConfigToken>.TokenType.EOF:
					return NewFixedToken(ConfigTokenType.EOF, startPosition, length, string.Empty);

				case AutoConfigScanner<ConfigToken>.TokenType.Label:
					return NewFixedToken(ConfigTokenType.Label, startPosition, length, expressionString.Substring(startPosition, length));

				case AutoConfigScanner<ConfigToken>.TokenType.OpenBrace:
					return NewFixedToken(ConfigTokenType.OpenBrace, startPosition, length, expressionString.Substring(startPosition, length));

				case AutoConfigScanner<ConfigToken>.TokenType.Regex:
					return NewFixedToken(ConfigTokenType.Regex, startPosition, length, expressionString.Substring(startPosition + 1, length - 2));

				case AutoConfigScanner<ConfigToken>.TokenType.Semicolon:
					return NewFixedToken(ConfigTokenType.Semicolon, startPosition, length, expressionString.Substring(startPosition, length));

				case AutoConfigScanner<ConfigToken>.TokenType.State:
					return NewFixedToken(ConfigTokenType.StateKeyword, startPosition, length, "state");

				case AutoConfigScanner<ConfigToken>.TokenType.String:
					var builder = new StringBuilder();

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

				case AutoConfigScanner<ConfigToken>.TokenType.Token:
					return NewFixedToken(ConfigTokenType.TokenKeyword, startPosition, length, "token");

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
