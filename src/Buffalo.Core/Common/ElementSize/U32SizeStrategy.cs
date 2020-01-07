// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Common
{
	sealed class U32SizeStrategy : ElementSizeStrategy
	{
		U32SizeStrategy()
		{
		}

		public static ElementSizeStrategy Instance { get; } = new U32SizeStrategy();

		public override string Keyword => "int";
		public override string BinaryReaderMember => nameof(BinaryReader.ReadInt32);
		public override int MaxValue => int.MaxValue;
		public override int Size(int elements) => elements << 2;

		public override void CopyBytes(int[] source, int sourceOffset, byte[] dest, int destOffset, int count)
		{
			if (sourceOffset < 0) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			if (sourceOffset > (source.Length << 2)) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
			if (destOffset < 0) throw new ArgumentOutOfRangeException(nameof(destOffset));
			if (destOffset > dest.Length) throw new ArgumentOutOfRangeException(nameof(destOffset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count + sourceOffset > (source.Length << 2)) throw new ArgumentOutOfRangeException(nameof(count));
			if (count + destOffset > dest.Length) throw new ArgumentOutOfRangeException(nameof(count));

			Buffer.BlockCopy(source, sourceOffset, dest, destOffset, count);
		}
	}
}
