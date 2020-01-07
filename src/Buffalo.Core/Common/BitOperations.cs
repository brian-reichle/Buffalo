// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Common
{
	static class BitOperations
	{
		/// <summary>
		/// Turn on all bits less significant than the most significant on bit.
		/// </summary>
		public static int CopyBitsDown(int value)
		{
			var tmp = unchecked((uint)value);
			tmp |= tmp >> 1;
			tmp |= tmp >> 2;
			tmp |= tmp >> 4;
			tmp |= tmp >> 8;
			tmp |= tmp >> 16;
			return unchecked((int)tmp);
		}

		/// <summary>
		/// Count the number of on bits in the given value.
		/// </summary>
		public static int PopulationCount(int value)
		{
			var tmp = unchecked((uint)value);
			tmp -= (tmp >> 1) & 0x55555555;
			tmp = ((tmp >> 2) & 0x33333333) + (tmp & 0x33333333);
			tmp = (((tmp + (tmp >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
			return unchecked((int)tmp);
		}
	}
}
