// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using NUnit.Framework;

namespace Buffalo.Main.Test
{
	[TestFixture]
	public class RangeMapTest
	{
		[Test]
		public void Clear()
		{
			var map = NewMap();

			map.Clear();

			Assert.That(new List<Range>(map), Is.EquivalentTo(System.Array.Empty<Range>()));
		}

		[Test]
		public void Set()
		{
			var map = new RangeMap();

			map.Set(10, 5);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 5) }));

			map.Set(20, 5);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 5), new Range(20, 5) }));

			map.Set(15, 5);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 15) }));

			map.Set(5, 25);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(5, 25) }));
		}

		[Test]
		public void Set2()
		{
			var map = NewMap();

			/*
			 * 0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
			 * 0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5
			 * .....*****.....*****.....*****.....*****.....*****.....*****.....*****.....*****......
			 * ....................*********************************************.....................
			 * .....*****.....*******************************************************.....*****......
			 */

			map.Set(20, 45);

			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(5, 5), new Range(15, 55), new Range(75, 5) }));
		}

		[Test]
		public void Unset()
		{
			var map = new RangeMap();

			map.Set(10, 50);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 50) }));

			map.Unset(20, 10);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(30, 30) }));

			map.Unset(40, 10);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(30, 10), new Range(50, 10) }));

			map.Unset(15, 40);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 5), new Range(55, 5) }));
		}

		[Test]
		public void Unset2()
		{
			var map = NewMap();

			/*
			 * 0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
			 * 0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5
			 * .....*****.....*****.....*****.....*****.....*****.....*****.....*****.....*****......
			 * .........................***********************************..........................
			 * .....*****.................................................................*****......
			 */

			map.Unset(25, 35);

			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[]
			{
				new Range(5, 5),
				new Range(15, 5),
				new Range(65, 5),
				new Range(75, 5),
			}));
		}

		[Test]
		public void Insert()
		{
			var map = new RangeMap();

			map.Set(10, 10);
			map.Set(30, 10);
			map.Set(50, 10);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(30, 10), new Range(50, 10) }));

			map.Insert(35, 10);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(30, 20), new Range(60, 10) }));

			map.Insert(25, 10);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(40, 20), new Range(70, 10) }));
		}

		[Test]
		public void Insert2()
		{
			var map = NewMap();

			/*
			 * 0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
			 * 0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5
			 * .....*****.....*****.....*****.....*****.....*****.....*****.....*****.....*****......
			 * ...........................=====......................................................
			 * .....*****.....*****.....**********.....*****.....*****.....*****.....*****.....*****.
			 */

			map.Insert(27, 5);

			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[]
			{
				new Range(5, 5),
				new Range(15, 5),
				new Range(25, 10),
				new Range(40, 5),
				new Range(50, 5),
				new Range(60, 5),
				new Range(70, 5),
				new Range(80, 5),
			}));
		}

		[Test]
		public void Delete()
		{
			var map = new RangeMap();

			map.Set(10, 10);
			map.Set(30, 10);
			map.Set(50, 10);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(30, 10), new Range(50, 10) }));

			map.Delete(35, 5);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(30, 5), new Range(45, 10) }));

			map.Delete(20, 5);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10), new Range(25, 5), new Range(40, 10) }));

			map.Delete(15, 30);
			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[] { new Range(10, 10) }));
		}

		[Test]
		public void Delete2()
		{
			var map = NewMap();

			/*
			 * 0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
			 * 0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5
			 * .....*****.....*****.....*****.....*****.....*****.....*****.....*****.....*****......
			 * .........................=========================....................................
			 * .....*****.....*****..........*****.....*****.....*****...............................
			 */

			map.Delete(25, 25);

			Assert.That(new List<Range>(map), Is.EquivalentTo(new Range[]
			{
				new Range(5, 5),
				new Range(15, 5),
				new Range(30, 5),
				new Range(40, 5),
				new Range(50, 5),
			}));
		}

		[Test]
		public void EnumeratorRange()
		{
			var map = NewMap();

			/*
			 * 0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
			 * 0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5
			 * .....*****.....*****.....*****.....*****.....*****.....*****.....*****.....*****......
			 * .............................[-------------------------]..............................
			 */

			var ranges = new List<Range>();

			using (var enumerator = map.GetEnumerator(29, 55))
			{
				while (enumerator.MoveNext())
				{
					ranges.Add(enumerator.Current);
				}
			}

			Assert.That(ranges, Is.EquivalentTo(new Range[]
			{
				new Range(29, 1),
				new Range(35, 5),
				new Range(45, 5),
				new Range(55, 1),
			}));
		}

		RangeMap NewMap()
		{
			/*
			 * 0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
			 * 0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5    0    5
			 * .....*****.....*****.....*****.....*****.....*****.....*****.....*****.....*****......
			 * ......................................................................................
			 * ......................................................................................
			 *
			 *                      75
			 * 05    25    45    65
			 *    15          55
			 *          35
			 */

			var result = new RangeMap();

			foreach (var start in new int[] { 35, 15, 55, 5, 25, 45, 65, 75 })
			{
				result.Set(start, 5);
			}

			return result;
		}
	}
}
