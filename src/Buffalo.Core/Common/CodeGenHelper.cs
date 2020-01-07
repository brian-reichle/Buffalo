// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;

namespace Buffalo.Core.Common
{
	static class CodeGenHelper
	{
		public static void WriteIndent(TextWriter writer, int indent)
		{
			for (var i = 0; i < indent; i++)
			{
				writer.Write('\t');
			}
		}

		public static void WriteLargeCharArray(TextWriter writer, int indent, int wrap, IList<char> data)
		{
			writer.Write("new char[");
			writer.Write(data.Count);
			writer.WriteLine(']');
			WriteIndent(writer, indent);
			writer.Write('{');

			var cutOff = 0;
			for (var i = 0; i < data.Count; i++)
			{
				if (cutOff == 0)
				{
					writer.WriteLine();
					WriteIndent(writer, indent + 1);
					writer.Write('\'');
					cutOff = wrap;
				}
				else
				{
					writer.Write(" '");
				}

				CharEscapeHelper.WriteEscapedChar(writer, data[i]);
				writer.Write("',");
				cutOff--;
			}

			writer.WriteLine();
			WriteIndent(writer, indent);
			writer.Write('}');
		}

		public static void WriteLargeIntArray(TextWriter writer, int indent, int wrap, CompressedBlob data)
		{
			switch (data.Method)
			{
				case Compression.CTB:
				case Compression.Simple:
					writer.Write("Expand(");
					WriteLargeIntArray(writer, indent, wrap, data, data.BlobSize);
					writer.Write(')');
					break;

				case Compression.None:
					WriteLargeIntArray(writer, indent, wrap, data, data.ElementSize);
					break;

				default:
					throw new InvalidOperationException("Unsupported compression method");
			}
		}

		public static void WriteLargeIntArray(TextWriter writer, int indent, int wrap, IList<int> data, ElementSizeStrategy elementSize)
		{
			writer.Write("new ");
			writer.Write(elementSize.Keyword);
			writer.Write('[');
			writer.Write(data.Count);
			writer.WriteLine(']');
			WriteIndent(writer, indent);
			writer.Write('{');

			var cutOff = 0;
			for (var i = 0; i < data.Count; i++)
			{
				if (cutOff == 0)
				{
					writer.WriteLine();
					WriteIndent(writer, indent + 1);
					cutOff = wrap;
				}
				else
				{
					writer.Write(' ');
				}

				writer.Write(data[i]);
				writer.Write(',');
				cutOff--;
			}

			writer.WriteLine();
			WriteIndent(writer, indent);
			writer.Write('}');
		}

		public static void WriteLargeIntArray(TextWriter writer, Compression method, string resourceName)
		{
			switch (method)
			{
				case Compression.CTB:
				case Compression.Simple:
					writer.Write("Expand(\"");
					writer.Write(resourceName);
					writer.Write("\")");
					break;

				case Compression.None:
					writer.Write("Extract(\"");
					writer.Write(resourceName);
					writer.Write("\")");
					break;

				default:
					throw new InvalidOperationException("Unsupported compression method");
			}
		}
	}
}
