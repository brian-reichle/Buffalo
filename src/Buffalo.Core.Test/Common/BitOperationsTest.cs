// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public class BitOperationsTest
	{
		[TestCase(0, ExpectedResult = 0)]
		[TestCase(1, ExpectedResult = 1)]
		[TestCase(-1, ExpectedResult = 32)]
		[TestCase(2, ExpectedResult = 1)]
		[TestCase(-2, ExpectedResult = 31)]
		[TestCase(unchecked((int)0x55555555), ExpectedResult = 16)]
		[TestCase(unchecked((int)0x33333333), ExpectedResult = 16)]
		[TestCase(unchecked((int)0x0F0F0F0F), ExpectedResult = 16)]
		[TestCase(unchecked((int)0xF0F0F0F0), ExpectedResult = 16)]
		[TestCase(unchecked((int)0x00FF00FF), ExpectedResult = 16)]
		[TestCase(unchecked((int)0xFF00FF00), ExpectedResult = 16)]
		[TestCase(unchecked((int)0x0000FFFF), ExpectedResult = 16)]
		[TestCase(unchecked((int)0xFFFF0000), ExpectedResult = 16)]
		public int PopulationCount(int value)
		{
			return BitOperations.PopulationCount(value);
		}

		[TestCase(0, ExpectedResult = 0)]
		[TestCase(1, ExpectedResult = 1)]
		[TestCase(-1, ExpectedResult = -1)]
		[TestCase(2, ExpectedResult = 3)]
		[TestCase(-2, ExpectedResult = -1)]
		[TestCase(40, ExpectedResult = 63)]
		public int CopyBitsDown(int value)
		{
			return BitOperations.CopyBitsDown(value);
		}
	}
}
