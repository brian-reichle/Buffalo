// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	sealed class MethodExtract : HelperMethod, IEquatable<MethodExtract>
	{
		public MethodExtract(ElementSizeStrategy sizeStrategy)
		{
			_sizeStrategy = sizeStrategy;
		}

		public override bool Equals(object obj) => Equals(obj as MethodExtract);
		public override bool Equals(HelperMethod other) => Equals(other as MethodExtract);
		public bool Equals(MethodExtract other) => other != null && other._sizeStrategy == _sizeStrategy;
		public override int GetHashCode() => HashCodeBuilder.Value + _sizeStrategy.GetHashCode() + typeof(MethodExtract).GetHashCode();

		public override void Write(TextWriter writer, int indent)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("static ");
			writer.Write(_sizeStrategy.Keyword);
			writer.WriteLine("[] Extract(string resourceName)");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("{");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine("using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))");
			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine('{');

			if (_sizeStrategy == U8SizeStrategy.Instance)
			{
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("byte[] result = new byte[stream.Length];");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("int offset = 0;");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("int read;");
				writer.WriteLine();
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("do");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine('{');
				CodeGenHelper.WriteIndent(writer, indent + 3);
				writer.WriteLine("read = stream.Read(result, offset, result.Length - offset);");
				CodeGenHelper.WriteIndent(writer, indent + 3);
				writer.WriteLine("offset += read;");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine('}');
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("while (offset < result.Length && read > 0);");
				writer.WriteLine();
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("return result;");
			}
			else
			{
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("int len = (int)stream.Length;");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.Write(_sizeStrategy.Keyword);
				writer.Write("[] result = new ");
				writer.Write(_sizeStrategy.Keyword);
				writer.Write("[len >> ");
				writer.Write(MeasureShift(_sizeStrategy.Size(1)));
				writer.WriteLine("];");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("byte[] buffer = new byte[Math.Min(stream.Length, 512)];");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("int offset = 0;");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("int read;");
				writer.WriteLine();
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("do");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("{");
				CodeGenHelper.WriteIndent(writer, indent + 3);
				writer.WriteLine("read = stream.Read(buffer, 0, buffer.Length);");
				CodeGenHelper.WriteIndent(writer, indent + 3);
				writer.WriteLine("Buffer.BlockCopy(buffer, 0, result, offset, read);");
				CodeGenHelper.WriteIndent(writer, indent + 3);
				writer.WriteLine("offset += read;");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("}");
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("while (offset < len && read > 0);");
				writer.WriteLine();
				CodeGenHelper.WriteIndent(writer, indent + 2);
				writer.WriteLine("return result;");
			}

			CodeGenHelper.WriteIndent(writer, indent + 1);
			writer.WriteLine('}');
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("}");
		}

		static int MeasureShift(int width)
		{
			var n = 0;
			width >>= 1;

			while (width > 0)
			{
				n++;
				width >>= 1;
			}

			return n;
		}

		readonly ElementSizeStrategy _sizeStrategy;
	}
}
