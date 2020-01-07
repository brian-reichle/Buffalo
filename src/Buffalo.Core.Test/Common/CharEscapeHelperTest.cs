// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class CharEscapeHelperTest
	{
		[Test]
		public void Write()
		{
			using (var writer = new StringWriter())
			{
				for (var i = 0; i < InChars.Length; i++)
				{
					CharEscapeHelper.WriteEscapedChar(writer, InChars[i]);
				}

				Assert.That(writer.ToString(), Is.EqualTo(OutChars));
			}
		}

		[Test]
		public void Append()
		{
			var builder = new StringBuilder();

			for (var i = 0; i < InChars.Length; i++)
			{
				CharEscapeHelper.AppendEscapedChar(builder, InChars[i]);
			}

			Assert.That(builder.ToString(), Is.EqualTo(OutChars));
		}

		const string InChars = "\\\'\0\a\b\f\n\r\t\v\"cat\u2022\u0001";
		const string OutChars = "\\\\\\'\\0\\a\\b\\f\\n\\r\\t\\v\"cat\\u2022\\u0001";
	}
}
