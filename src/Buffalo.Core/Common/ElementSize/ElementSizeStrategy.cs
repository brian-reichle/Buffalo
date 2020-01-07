// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	[DebuggerDisplay("{Keyword}")]
	abstract class ElementSizeStrategy
	{
		public static ElementSizeStrategy Get(TableElementSize size)
		{
			switch (size)
			{
				case TableElementSize.Int: return U32SizeStrategy.Instance;
				case TableElementSize.Short: return U16SizeStrategy.Instance;
				case TableElementSize.Byte: return U8SizeStrategy.Instance;
				default: throw new ArgumentOutOfRangeException(nameof(size), size, "Invalid table element size");
			}
		}

		public abstract string Keyword { get; }
		public abstract string BinaryReaderMember { get; }
		public abstract int MaxValue { get; }
		public abstract int Size(int elements);
		public abstract void CopyBytes(int[] source, int sourceOffset, byte[] dest, int destOffset, int count);
	}
}
