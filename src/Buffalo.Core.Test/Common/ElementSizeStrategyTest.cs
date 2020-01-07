// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class ElementSizeStrategyTest
	{
		[Test]
		public void Get()
		{
			Assert.That(ElementSizeStrategy.Get(TableElementSize.Byte), Is.SameAs(U8SizeStrategy.Instance));
			Assert.That(ElementSizeStrategy.Get(TableElementSize.Short), Is.SameAs(U16SizeStrategy.Instance));
			Assert.That(ElementSizeStrategy.Get(TableElementSize.Int), Is.SameAs(U32SizeStrategy.Instance));
		}

		[Test]
		public void MaxSize()
		{
			Assert.That(U8SizeStrategy.Instance.MaxValue, Is.EqualTo(byte.MaxValue));
			Assert.That(U16SizeStrategy.Instance.MaxValue, Is.EqualTo(ushort.MaxValue));
			Assert.That(U32SizeStrategy.Instance.MaxValue, Is.EqualTo(int.MaxValue));
		}

		[Test]
		public void Keyword()
		{
			Assert.That(U8SizeStrategy.Instance.Keyword, Is.EqualTo("byte"));
			Assert.That(U16SizeStrategy.Instance.Keyword, Is.EqualTo("ushort"));
			Assert.That(U32SizeStrategy.Instance.Keyword, Is.EqualTo("int"));
		}

		[Test]
		public void BinaryReaderMember()
		{
			Assert.That(U8SizeStrategy.Instance.BinaryReaderMember, Is.EqualTo("ReadByte"));
			Assert.That(U16SizeStrategy.Instance.BinaryReaderMember, Is.EqualTo("ReadUInt16"));
			Assert.That(U32SizeStrategy.Instance.BinaryReaderMember, Is.EqualTo("ReadInt32"));
		}

		[Test]
		public void Size()
		{
			Assert.That(U8SizeStrategy.Instance.Size(1), Is.EqualTo(1));
			Assert.That(U16SizeStrategy.Instance.Size(1), Is.EqualTo(2));
			Assert.That(U32SizeStrategy.Instance.Size(1), Is.EqualTo(4));

			Assert.That(U8SizeStrategy.Instance.Size(10), Is.EqualTo(10));
			Assert.That(U16SizeStrategy.Instance.Size(10), Is.EqualTo(20));
			Assert.That(U32SizeStrategy.Instance.Size(10), Is.EqualTo(40));
		}

		[Test]
		public void CopyBytes8()
		{
			var inValue = new int[]
			{
				0x00,
				0x10,
				0xAAFF,
				0x1BBEECC,
			};

			var expectedOutValue1 = new byte[]
			{
				0x00,
				0x10,
				0xFF,
				0xCC,
			};

			var expectedOutValue2 = new byte[]
			{
				0x00,
				0x00,
				0x10,
				0xFF,
			};

			var buffer = new byte[inValue.Length];

			U8SizeStrategy.Instance.CopyBytes(inValue, 0, buffer, 0, 4);
			Assert.That(buffer, Is.EquivalentTo(expectedOutValue1));

			Array.Clear(buffer, 0, buffer.Length);
			U8SizeStrategy.Instance.CopyBytes(inValue, 1, buffer, 2, 2);
			Assert.That(buffer, Is.EquivalentTo(expectedOutValue2));
		}

		[Test]
		public void CopyBytes16()
		{
			var inValue = new int[]
			{
				0x00,
				0x10,
				0xAAFF,
				0x1BBEECC,
			};

			var expectedOutValue1 = new byte[]
			{
				0x00, 0x00,
				0x10, 0x00,
				0xFF, 0xAA,
				0xCC, 0xEE,
			};

			var expectedOutValue2 = new byte[]
			{
				0x00, 0x00,
				0x00, 0xFF,
				0xAA, 0xCC,
				0x00, 0x00,
			};

			var buffer = new byte[inValue.Length << 1];

			U16SizeStrategy.Instance.CopyBytes(inValue, 0, buffer, 0, 8);
			Assert.That(buffer, Is.EquivalentTo(expectedOutValue1));

			Array.Clear(buffer, 0, buffer.Length);
			U16SizeStrategy.Instance.CopyBytes(inValue, 3, buffer, 2, 4);
			Assert.That(buffer, Is.EquivalentTo(expectedOutValue2));
		}

		[Test]
		public void CopyBytes32()
		{
			var inValue = new int[]
			{
				0x00,
				0x10,
				0xAAFF,
				0x1BBEECC,
			};

			var expectedOutValue1 = new byte[]
			{
				0x00, 0x00, 0x00, 0x00,
				0x10, 0x00, 0x00, 0x00,
				0xFF, 0xAA, 0x00, 0x00,
				0xCC, 0xEE, 0xBB, 0x01,
			};

			var expectedOutValue2 = new byte[]
			{
				0x00, 0x00, 0x00, 0xFF,
				0xAA, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
			};

			var buffer = new byte[inValue.Length << 2];

			U32SizeStrategy.Instance.CopyBytes(inValue, 0, buffer, 0, 16);
			Assert.That(buffer, Is.EquivalentTo(expectedOutValue1));

			Array.Clear(buffer, 0, buffer.Length);
			U32SizeStrategy.Instance.CopyBytes(inValue, 6, buffer, 2, 4);
			Assert.That(buffer, Is.EquivalentTo(expectedOutValue2));
		}
	}
}
