// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Globalization;
using System.IO;
using System.Text;

namespace Buffalo.Core.Common
{
	static class CharEscapeHelper
	{
		public static void AppendEscapedChar(StringBuilder builder, char c)
		{
			using (var writer = new StringWriter(builder))
			{
				WriteEscapedChar(writer, c);
				writer.Flush();
			}
		}

		public static void WriteEscapedChar(TextWriter writer, char c)
		{
			switch (c)
			{
				case '\\':
					writer.Write("\\\\");
					break;

				case '\'':
					writer.Write("\\'");
					break;

				case '\0':
					writer.Write("\\0");
					break;

				case '\a':
					writer.Write("\\a");
					break;

				case '\b':
					writer.Write("\\b");
					break;

				case '\f':
					writer.Write("\\f");
					break;

				case '\n':
					writer.Write("\\n");
					break;

				case '\r':
					writer.Write("\\r");
					break;

				case '\t':
					writer.Write("\\t");
					break;

				case '\v':
					writer.Write("\\v");
					break;

				default:
					if (c >= 32 && c <= 127)
					{
						writer.Write(c);
					}
					else
					{
						writer.Write("\\u");
						writer.Write(((int)c).ToString("x4", CultureInfo.InvariantCulture));
					}

					break;
			}
		}
	}
}
