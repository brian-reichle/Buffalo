// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	sealed class MethodExpandCTB : HelperMethod, IEquatable<MethodExpandCTB>
	{
		public MethodExpandCTB(ElementSizeStrategy sizeStrategy, bool fromResource)
		{
			_fromResource = fromResource;
			_sizeStrategy = sizeStrategy;
		}

		public override bool Equals(object obj) => Equals(obj as MethodExpandCTB);
		public override bool Equals(HelperMethod other) => Equals(other as MethodExpandCTB);

		public bool Equals(MethodExpandCTB other)
			=> other != null && other._fromResource == _fromResource && other._sizeStrategy == _sizeStrategy;

		public override int GetHashCode()
			=> HashCodeBuilder.Value + _sizeStrategy.GetHashCode() + typeof(MethodExpandCTB).GetHashCode() + _fromResource.GetHashCode();

		public override void Write(TextWriter writer, int indent)
		{
			if (_fromResource)
			{
				WriteWithExtract(writer, indent);
			}
			else
			{
				WriteWithoutExtract(writer, indent);
			}
		}

		void WriteWithoutExtract(TextWriter writer, int indent)
		{
			const string readLine = "tmp = collapsed[read++];";
			const string notDoneCheck = "read < collapsed.Length";

			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("static ");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine("[] Expand(byte[] collapsed)");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("int read = 0;");
			writer.WriteLine();
			WriteCore(writer, indent + 1, readLine, notDoneCheck);
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("return result;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("}");
		}

		void WriteWithExtract(TextWriter writer, int indent)
		{
			const string readLine = "tmp = unchecked((byte)stream.ReadByte());";
			const string notDoneCheck = "stream.Position < stream.Length";

			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("static ");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine("[] Expand(string resourceName)");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("{");
			WriteCore(writer, indent + 2, readLine, notDoneCheck);
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("return result;");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("}");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("}");
		}

		void WriteCore(TextWriter writer, int indent, string readLine, string notDoneCheck)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("const byte FOLLOW = 0x80;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("const byte REPEAT = 0x40;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("const byte FIRSTBODY = 0x3F;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("const byte SUBBODY = 0x7F;");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("int value;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("byte tmp;");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("value = tmp & FIRSTBODY;");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("while ((tmp & FOLLOW) != 0)");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("value = (value << 7) | (tmp & SUBBODY);");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("}");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("int write = 0;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(_sizeStrategy.Keyword);
			writer.Write("[] result = new ");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine("[value];");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("while (");
			writer.Write(notDoneCheck);
			writer.WriteLine(")");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("if ((tmp & REPEAT) == 0)");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("value = tmp & FIRSTBODY;");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("while ((tmp & FOLLOW) != 0)");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine("value = (value << 7) | (tmp & SUBBODY);");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("}");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.Write("result[write++] = unchecked((");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine(")value);");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("}");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("else");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("int count = tmp & FIRSTBODY;");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("while ((tmp & FOLLOW) != 0)");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine("count = (count << 7) | (tmp & SUBBODY);");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("}");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("value = tmp & FIRSTBODY;");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("while ((tmp & FOLLOW) != 0)");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine(readLine);
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine("value = (value << 7) | (tmp & SUBBODY);");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("}");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("while (count > 0)");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.Write("result[write++] = unchecked((");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine(")value);");
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine("count--;");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("}");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("}");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("}");
		}

		readonly bool _fromResource;
		readonly ElementSizeStrategy _sizeStrategy;
	}
}
