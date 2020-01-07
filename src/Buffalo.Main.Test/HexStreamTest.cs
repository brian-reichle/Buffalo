// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Buffalo.Main.Test
{
	[TestFixture]
	public class HexStreamTest
	{
		[Test]
		public void TestWriteFull()
		{
			const string expected =
				"00000000 0D 0A 09 09 09 4C 69 74 74 6C 65 20 73 6C 61 62 .....Little slab\r\n" +
				"00000010 20 6F 66 20 6D 65 61 74 0D 0A 09 09 09 49 6E 20  of meat.....In \r\n" +
				"00000020 61 20 77 61 73 68 20 6F 66 20 63 6C 65 61 72 20 a wash of clear \r\n" +
				"00000030 6A 65 6C 6C 79 0D 0A 09 09 09 4E 6F 77 20 49 20 jelly.....Now I \r\n" +
				"00000040 68 65 61 74 20 74 68 65 20 70 61 6E 0D 0A 09 09 heat the pan....\r\n" +
				"00000050 09                                              .               \r\n" +
				"";

			using (var writer = new StringWriter())
			using (var stream = new HexStream(writer))
			{
				var buffer = Encoding.UTF8.GetBytes(Haiku);

				stream.Write(buffer, 0, buffer.Length);
				stream.Flush();

				Assert.That(writer.ToString(), Is.EqualTo(expected));
			}
		}

		[Test]
		public void TestWriteSplit()
		{
			const string expected =
				"00000000 0D 0A 09 09 09 4C 69 74 74 6C 65 20 73 6C 61 62 .....Little slab\r\n" +
				"00000010 20 6F 66 20 6D 65 61 74                          of meat        \r\n" +
				"00000010                         0D 0A 09 09 09 49 6E 20         .....In \r\n" +
				"00000020 61 20 77 61 73 68 20 6F 66 20 63 6C 65 61 72 20 a wash of clear \r\n" +
				"00000030 6A 65 6C 6C 79 0D 0A 09 09 09 4E 6F 77 20 49 20 jelly.....Now I \r\n" +
				"00000040 68 65 61 74 20 74 68 65 20 70 61 6E 0D 0A 09 09 heat the pan....\r\n" +
				"00000050 09                                              .               \r\n" +
				"";

			using (var writer = new StringWriter())
			using (var stream = new HexStream(writer))
			{
				var buffer = Encoding.UTF8.GetBytes(Haiku);

				stream.Write(buffer, 0, 24);
				stream.Flush();
				stream.Write(buffer, 24, 24);
				stream.Flush();
				stream.Write(buffer, 48, buffer.Length - 48);
				stream.Flush();

				Assert.That(writer.ToString(), Is.EqualTo(expected));
			}
		}

		const string Haiku = @"
			Little slab of meat
			In a wash of clear jelly
			Now I heat the pan
			";
	}
}
