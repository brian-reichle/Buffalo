// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class TableFragmentTest
	{
		[Test]
		public void Combine()
		{
			var fragment1 = new TableFragment(new int[] { 1, 2, 3, -1 }, 0);
			var fragment2 = new TableFragment(new int[] { 1, 2, 3, -1 }, 1);
			var fragment3 = new TableFragment(new int[] { -1, -1, 2, -1 }, 2);
			var fragment4 = new TableFragment(new int[] { -1, 1, -1, -1 }, 3);

			var combined = TableFragment.Combine(new TableFragment[]
			{
				fragment1,
				fragment2,
				fragment3,
				fragment4,
			});

			Assert.That(combined.Skip, Is.EqualTo(0));
			Assert.That(combined, Is.EquivalentTo(new int[] { 1, 2, 3, -1, 1, -1, -1, 2, -1 }));
			Assert.That(combined.GetOffset(0), Is.EqualTo(0));
			Assert.That(combined.GetOffset(1), Is.EqualTo(0));
			Assert.That(combined.GetOffset(2), Is.EqualTo(5));
			Assert.That(combined.GetOffset(3), Is.EqualTo(3));
			Assert.That(combined.GetOffset(4), Is.EqualTo(null));
		}

		[Test]
		public void Skip_NoExcess()
		{
			var fragment1 = new TableFragment(new int[] { 1, 1, 2, 2 }, 0);
			var fragment2 = new TableFragment(new int[] { 2 }, 1, 2);

			var combined = TableFragment.Combine(new TableFragment[]
			{
				fragment1,
				fragment2,
			});

			Assert.That(combined.Skip, Is.EqualTo(0));
			Assert.That(combined, Is.EquivalentTo(new int[] { 1, 1, 2, 2 }));
			Assert.That(combined.GetOffset(0), Is.EqualTo(0));
			Assert.That(combined.GetOffset(1), Is.EqualTo(1));
		}

		[Test]
		public void Skip_WithExcess()
		{
			var fragment1 = new TableFragment(new int[] { 1, 1, 2, 2 }, 0);
			var fragment2 = new TableFragment(new int[] { 2, 2, 3 }, 1, 0);
			var fragment3 = new TableFragment(new int[] { 3 }, 2, 5);

			var combined = TableFragment.Combine(new TableFragment[]
			{
				fragment1,
				fragment2,
				fragment3,
			});

			Assert.That(combined.Skip, Is.EqualTo(1));
			Assert.That(combined, Is.EquivalentTo(new int[] { 1, 1, 2, 2, 3 }));
			Assert.That(combined.GetOffset(0), Is.EqualTo(0));
			Assert.That(combined.GetOffset(1), Is.EqualTo(2));
			Assert.That(combined.GetOffset(2), Is.EqualTo(-1));
		}

		[Test]
		public void CopyTo()
		{
			var fragment = new TableFragment(new int[] { 1, 2, 3, 4 }, 0);

			var array = new int[6];
			fragment.CopyTo(array, 1);

			Assert.That(array, Is.EquivalentTo(new int[] { 0, 1, 2, 3, 4, 0 }));
		}

		[Test]
		public void AppendAfterDiscardingStart()
		{
			var table = new int[][]
			{
				new int[] { -1, 2, },
				null,
				new int[] { 2, 3, },
			};

			CheckIntegrity(table, 3);
		}

		[Test]
		public void AppendAfterDiscardingEnd()
		{
			var table = new int[][]
			{
				new int[] { 2, -1, },
				null,
				new int[] { 3, 2, },
			};

			CheckIntegrity(table, 3);
		}

		[Test]
		public void AppendBeforeDiscarding()
		{
			var table = new int[][]
			{
				new int[] { 0, 1, 2, },
				new int[] { 2, 3, 3, },
				new int[] { 4, 4, 0, },
			};

			CheckIntegrity(table, 7);
		}

		[Test]
		public void Unconnected()
		{
			var table = new int[][]
			{
				new int[] { 1, 1, 1, },
				new int[] { 2, 2, 2, },
				new int[] { 3, 3, 3, },
			};

			CheckIntegrity(table, 9);
		}

		[Test]
		public void TailFallback()
		{
			var table = new int[][]
			{
				new int[] { 2, -1, },
				new int[] { 2, 2, },
			};

			CheckIntegrity(table, 3);
		}

		[Test]
		public void HeadFallback()
		{
			var table = new int[][]
			{
				new int[] { -1, 2, },
				new int[] { 2, 2, },
			};

			CheckIntegrity(table, 3);
		}

		[Test]
		public void HashConflict()
		{
			var table = new int[][]
			{
				new int[] { 1, 0, },
				new int[] { 0, 1 << 27, },
			};

			CheckIntegrity(table, 3);
		}

		[Test]
		public void Duplicates()
		{
			var table = new int[][]
			{
				new int[] { 1, 2, },
				new int[] { 1, 2, },
				new int[] { 1, 2, },
			};

			CheckIntegrity(table, 2);
		}

		void CheckIntegrity(int[][] table, int expectedLen)
		{
			var fragments = new List<TableFragment>();

			for (var s = 0; s < table.Length; s++)
			{
				var row = table[s];
				if (row == null) continue;

				fragments.Add(new TableFragment(row, s));
			}

			var compound = TableFragment.Combine(fragments);

			var builder = new StringBuilder();

			for (var s = 0; s < table.Length; s++)
			{
				var offset = compound.GetOffset(s);
				var row = table[s];

				if (row == null)
				{
					if (offset >= 0)
					{
						builder.Append("row[");
						builder.Append(s);
						builder.Append("] should be null but was ");
						builder.Append(offset);
						builder.AppendLine();
					}
				}
				else if (!offset.HasValue)
				{
					builder.Append("row[");
					builder.Append(s);
					builder.Append("] should be non-null but was null");
					builder.AppendLine();
				}
				else
				{
					for (var c = 0; c < row.Length; c++)
					{
						var expected = row[c];
						var actual = compound[offset.Value + c];

						if (expected != actual)
						{
							builder.Append("row[");
							builder.Append(s);
							builder.Append("][");
							builder.Append(c);
							builder.Append("] should be ");
							builder.Append(expected);
							builder.Append(" but was ");
							builder.Append(actual);
							builder.AppendLine();
						}
					}
				}
			}

			Assert.That(builder.ToString(), Is.EqualTo(string.Empty));
			Assert.That(compound.Count, Is.EqualTo(expectedLen));
		}
	}
}
