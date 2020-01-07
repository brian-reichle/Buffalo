// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public class CompressedBlobTest
	{
		[Test]
		public void None([ValueSource(nameof(_sizeValues))] int size)
		{
			var sizex = ElementSizeStrategy.Get((TableElementSize)size);

			var cb = CompressedBlob.Compress(Compression.None, sizex, _sampleBlob1);
			Assert.That(cb.Method, Is.EqualTo(Compression.None));
			Assert.That(cb.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb.BlobSize, Is.EqualTo(sizex));
			Assert.That(cb, Is.EquivalentTo(_sampleBlob1));
		}

		[Test]
		public void CTB([ValueSource(nameof(_sizeValues))] int size)
		{
			var sizex = ElementSizeStrategy.Get((TableElementSize)size);

			var cb1 = CompressedBlob.Compress(Compression.CTB, sizex, _sampleBlob1);
			Assert.That(cb1.Method, Is.EqualTo(Compression.CTB));
			Assert.That(cb1.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb1.BlobSize, Is.EqualTo(U8SizeStrategy.Instance));
			Assert.That(cb1, Is.EquivalentTo(_sampleBlob1CTB));

			var cb2 = CompressedBlob.Compress(Compression.CTB, sizex, _sampleBlob2);
			Assert.That(cb2.Method, Is.EqualTo(Compression.CTB));
			Assert.That(cb2.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb2.BlobSize, Is.EqualTo(U8SizeStrategy.Instance));
			Assert.That(cb2, Is.EquivalentTo(_sampleBlob2CTB));
		}

		[Test]
		public void Simple([ValueSource(nameof(_sizeValues))] int size)
		{
			var sizex = ElementSizeStrategy.Get((TableElementSize)size);

			var cb1 = CompressedBlob.Compress(Compression.Simple, sizex, _sampleBlob1);
			Assert.That(cb1.Method, Is.EqualTo(Compression.Simple));
			Assert.That(cb1.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb1.BlobSize, Is.EqualTo(sizex));
			Assert.That(cb1, Is.EquivalentTo(Replace(_sampleBlob1Simple, -1, sizex.MaxValue)));

			var cb2 = CompressedBlob.Compress(Compression.Simple, sizex, _sampleBlob2);
			Assert.That(cb2.Method, Is.EqualTo(Compression.Simple));
			Assert.That(cb2.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb2.BlobSize, Is.EqualTo(sizex));
			Assert.That(cb2, Is.EquivalentTo(Replace(_sampleBlob2Simple, -1, sizex.MaxValue)));
		}

		[Test]
		public void Auto([ValueSource(nameof(_sizeValues))] int size)
		{
			var sizex = ElementSizeStrategy.Get((TableElementSize)size);

			var cb1 = CompressedBlob.Compress(Compression.Auto, sizex, _sampleBlob1);
			Assert.That(cb1.Method, Is.EqualTo(Compression.Simple));
			Assert.That(cb1.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb1.BlobSize, Is.EqualTo(sizex));
			Assert.That(cb1, Is.EquivalentTo(Replace(_sampleBlob1Simple, -1, sizex.MaxValue)));

			var cb2 = CompressedBlob.Compress(Compression.Auto, sizex, _sampleBlob2);
			Assert.That(cb2.Method, Is.EqualTo(Compression.None));
			Assert.That(cb2.ElementSize, Is.EqualTo(sizex));
			Assert.That(cb2.BlobSize, Is.EqualTo(sizex));
			Assert.That(cb2, Is.EquivalentTo(_sampleBlob2));
		}

		[Test]
		public void CopyBytesTo()
		{
			var src = new int[]
			{
				0x01, 0x02, 0x03, 0x04,
				0x11, 0x12, 0x13, 0x14,
				0x21, 0x22, 0x23, 0x24,
				0x31, 0x32, 0x33, 0x34,
			};

			var expected = new byte[]
			{
				0x12, 0x13, 0x14, 0x21,
				0x22, 0x00, 0x00, 0x00,
				0x00, 0x00,
			};

			var cb1 = CompressedBlob.Compress(Compression.None, U8SizeStrategy.Instance, src);
			var buffer = new byte[10];
			cb1.CopyBytesTo(buffer, 5, 5);

			Assert.That(buffer, Is.EquivalentTo(expected));
		}

		#region Implementation

		static int[] Replace(int[] blob, int from, int to)
		{
			var result = new int[blob.Length];

			for (var i = 0; i < blob.Length; i++)
			{
				var v = blob[i];
				result[i] = v == from ? to : v;
			}

			return result;
		}

		static readonly IEnumerable<int> _sizeValues = new int[]
		{
			(int)TableElementSize.Byte,
			(int)TableElementSize.Short,
			(int)TableElementSize.Int,
		};
		readonly int[] _sampleBlob1 = new int[30]
		{
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2,
			2, 2, 2, 70, 532544, 3, 1, 2, 1, 2, 1, 3, 0, 0,
		};
		readonly int[] _sampleBlob1CTB = new int[18]
		{
			30, 1, 74, 0, 72, 2, 128, 70, 160, 192, 64, 3, 1, 2, 1, 2, 1, 3,
		};
		readonly int[] _sampleBlob1Simple = new int[20]
		{
			30, -1, 1, -1, 10, 0, -1, 8, 2, 70, 532544, 3, 1, 2, 1, 2, 1, 3, 0, 0,
		};
		readonly int[] _sampleBlob2 = new int[36]
		{
			0, 9, 1, 2, 7, 8, 3, 0, 5, 8, 9, 7, 2, 3, 4, 0,
			9, 5, 8, 7, 2, 3, 4, 0, 8, 9, 5, 7, 2, 3, 4, 0,
			9, 8, 5, 7,
		};
		readonly int[] _sampleBlob2CTB = new int[37]
		{
			36, 0, 9, 1, 2, 7, 8, 3, 0, 5, 8, 9, 7, 2, 3, 4, 0,
			9, 5, 8, 7, 2, 3, 4, 0, 8, 9, 5, 7, 2, 3, 4, 0,
			9, 8, 5, 7,
		};
		readonly int[] _sampleBlob2Simple = new int[38]
		{
			36, -1, 0, 9, 1, 2, 7, 8, 3, 0, 5, 8, 9, 7, 2, 3,
			4, 0, 9, 5, 8, 7, 2, 3, 4, 0, 8, 9, 5, 7, 2, 3,
			4, 0, 9, 8, 5, 7,
		};

		#endregion
	}
}
