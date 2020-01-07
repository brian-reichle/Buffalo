// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	sealed class U8SizeStrategy : ElementSizeStrategy
	{
		U8SizeStrategy()
		{
		}

		public static ElementSizeStrategy Instance { get; } = new U8SizeStrategy();

		public override string Keyword => "byte";
		public override string BinaryReaderMember => nameof(BinaryReader.ReadByte);
		public override int MaxValue => byte.MaxValue;
		public override int Size(int elements) => elements;

		public override void CopyBytes(int[] source, int sourceOffset, byte[] dest, int destOffset, int count)
		{
			if (sourceOffset < 0) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			if (sourceOffset > (source.Length << 2)) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			if (destOffset < 0) throw new ArgumentOutOfRangeException(nameof(destOffset));
			if (destOffset > dest.Length) throw new ArgumentOutOfRangeException(nameof(destOffset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count + sourceOffset > (source.Length << 2)) throw new ArgumentOutOfRangeException(nameof(count));
			if (count + destOffset > dest.Length) throw new ArgumentOutOfRangeException(nameof(count));

			for (var i = 0; i < count; i++)
			{
				dest[destOffset + i] = unchecked((byte)source[sourceOffset + i]);
			}
		}
	}
}
