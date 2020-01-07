// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Buffalo.Core.Common;

namespace Buffalo.Core.Test
{
	static partial class Renderer
	{
		public static string Render(IEnumerable<Token> tokens)
		{
			var builder = new StringBuilder();

			builder.AppendLine("new Token[]");
			builder.AppendLine("{");

			foreach (var token in tokens)
			{
				builder.Append("\t");
				Render(builder, token);
				builder.AppendLine(",");
			}

			builder.AppendLine("}");

			return builder.ToString();
		}

		static void Render(StringBuilder builder, Token token)
		{
			builder.Append("new Token(");
			AppendString(builder, token.TokenTypeName);
			builder.Append(", ");
			AppendString(builder, token.Value);
			builder.Append(", ");
			Render(builder, token.FromPos);
			builder.Append(", ");
			Render(builder, token.ToPos);
			builder.Append(")");
		}

		static void Render(StringBuilder builder, CharPosX pos)
		{
			builder.Append("new CharPos(");
			builder.Append(pos.Index);
			builder.Append(", ");
			builder.Append(pos.LineNo);
			builder.Append(", ");
			builder.Append(pos.CharNo);
			builder.Append(")");
		}

		static void AppendString(StringBuilder builder, string s)
		{
			if (s == null)
			{
				builder.Append("null");
			}
			else
			{
				builder.Append("\"");

				for (var i = 0; i < s.Length; i++)
				{
					AppendEscapedChar(builder, s[i]);
				}

				builder.Append("\"");
			}
		}

		static void AppendEscapedChar(StringBuilder builder, char c)
		{
			switch (c)
			{
				case '\0':
					builder.Append("\\0");
					break;

				case '\\':
					builder.Append("\\\\");
					break;

				case '"':
					builder.Append("\\\"");
					break;

				case '\a':
					builder.Append("\\a");
					break;

				case '\b':
					builder.Append("\\b");
					break;

				case '\f':
					builder.Append("\\f");
					break;

				case '\t':
					builder.Append("\\t");
					break;

				case '\r':
					builder.Append("\\r");
					break;

				case '\n':
					builder.Append("\\n");
					break;

				case '\v':
					builder.Append("\\v");
					break;

				case ' ':
					builder.Append(" ");
					break;

				default:
					if (c >= 32 && c <= 127)
					{
						builder.Append(c);
					}
					else
					{
						builder.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c);
					}

					break;
			}
		}

		static void Render(StringBuilder builder, CharPos pos)
		{
			if (pos == null)
			{
				builder.Append("null");
			}
			else
			{
				builder.Append("new CharPos(");
				builder.Append(pos.Index);
				builder.Append(", ");
				builder.Append(pos.LineNo);
				builder.Append(", ");
				builder.Append(pos.CharNo);
				builder.Append(')');
			}
		}
	}
}
