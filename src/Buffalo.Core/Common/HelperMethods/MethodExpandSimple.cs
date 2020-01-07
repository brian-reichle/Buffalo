// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	sealed class MethodExpandSimple : HelperMethod, IEquatable<MethodExpandSimple>
	{
		public MethodExpandSimple(ElementSizeStrategy sizeStrategy, bool fromResource)
		{
			_fromResource = fromResource;
			_sizeStrategy = sizeStrategy;
		}

		public override bool Equals(object obj) => Equals(obj as MethodExpandSimple);
		public override bool Equals(HelperMethod other) => Equals(other as MethodExpandSimple);

		public bool Equals(MethodExpandSimple other)
			=> other != null && other._fromResource == _fromResource && other._sizeStrategy == _sizeStrategy;

		public override int GetHashCode()
			=> HashCodeBuilder.Value + _sizeStrategy.GetHashCode() + typeof(MethodExpandSimple).GetHashCode() + _fromResource.GetHashCode();

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
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("static ");
			writer.Write(_sizeStrategy.Keyword);
			writer.Write("[] Expand(");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine("[] collapsed)");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('{');
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("int i = 0;");
			WriteCore(writer, indent + 1, "collapsed[i++]", "i < collapsed.Length");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('}');
		}

		void WriteWithExtract(TextWriter writer, int indent)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("static ");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine("[] Expand(string resourceName)");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('{');
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("{");
			WriteCore(writer, indent + 2, "reader." + _sizeStrategy.BinaryReaderMember + "()", "stream.Position < stream.Length");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("}");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('}');
		}

		void WriteCore(TextWriter writer, int indent, string readValue, string notDoneCheck)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(_sizeStrategy.Keyword);
			writer.Write("[] result = new ");
			writer.Write(_sizeStrategy.Keyword);
			writer.Write('[');
			writer.Write(readValue);
			writer.WriteLine("];");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(_sizeStrategy.Keyword);
			writer.Write(" escape = ");
			writer.Write(readValue);
			writer.WriteLine(";");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("int w = 0;");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("while (");
			writer.Write(notDoneCheck);
			writer.WriteLine(")");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('{');
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.Write(_sizeStrategy.Keyword);
			writer.Write(" value = ");
			writer.Write(readValue);
			writer.WriteLine(";");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("if (value == escape)");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine('{');
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.Write(_sizeStrategy.Keyword);
			writer.Write(" count = ");
			writer.Write(readValue);
			writer.WriteLine(";");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.Write("value = ");
			writer.Write(readValue);
			writer.WriteLine(";");
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("while (count > 0)");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine('{');
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine("result[w++] = value;");
			CodeGenHelper.WriteIndent(writer, indent + 3);
			writer.WriteLine("count--;");
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine('}');
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine('}');
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("else");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine('{');
			CodeGenHelper.WriteIndent(writer, indent + 2);
			writer.WriteLine("result[w++] = value;");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine('}');
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('}');
			writer.WriteLine();
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("return result;");
		}

		readonly bool _fromResource;
		readonly ElementSizeStrategy _sizeStrategy;
	}
}
