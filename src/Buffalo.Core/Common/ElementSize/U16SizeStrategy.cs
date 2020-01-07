// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	sealed class U16SizeStrategy : ElementSizeStrategy
	{
		U16SizeStrategy()
		{
		}

		public static ElementSizeStrategy Instance { get; } = new U16SizeStrategy();

		public override string Keyword => "ushort";
		public override string BinaryReaderMember => nameof(BinaryReader.ReadUInt16);
		public override int MaxValue => ushort.MaxValue;
		public override int Size(int elements) => elements << 1;

		public override void CopyBytes(int[] source, int sourceOffset, byte[] dest, int destOffset, int count)
		{
			if (sourceOffset < 0) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			if (sourceOffset > (source.Length << 1)) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			if (destOffset < 0) throw new ArgumentOutOfRangeException(nameof(destOffset));
			if (destOffset > dest.Length) throw new ArgumentOutOfRangeException(nameof(destOffset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count + sourceOffset > (source.Length << 1)) throw new ArgumentOutOfRangeException(nameof(count));
			if (count + destOffset > dest.Length) throw new ArgumentOutOfRangeException(nameof(count));

			var n = sourceOffset + (sourceOffset & ~1);

			for (var i = 0; i < count; i++)
			{
				dest[destOffset + i] = Buffer.GetByte(source, n);

				n += ((n & 0x01) << 1) + 1;
			}
		}
	}
}
