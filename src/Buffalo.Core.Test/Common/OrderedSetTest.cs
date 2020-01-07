// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class OrderedSetTest
	{
		[Test]
		public void New()
		{
			var set1 = OrderedSet<int>.New(new int[] { 1, 2, 2 });
			var set2 = OrderedSet<int>.New(new int[] { 2, 1 });
			var set3 = OrderedSet<int>.New(new int[] { 2, 2 });

			Assert.That(set1.Equals(set2), Is.EqualTo(true));
			Assert.That(set2.Equals(set1), Is.EqualTo(true));

			Assert.That(set1.Equals(set3), Is.EqualTo(false));
			Assert.That(set3.Equals(set1), Is.EqualTo(false));

			Assert.That(set2.Equals(set3), Is.EqualTo(false));
			Assert.That(set3.Equals(set2), Is.EqualTo(false));
		}

		[Test]
		public void Length()
		{
			var set1 = OrderedSet<int>.New(System.Array.Empty<int>());
			Assert.That(set1.Length, Is.EqualTo(0));
			Assert.That(((ICollection)set1).Count, Is.EqualTo(0));

			var set2 = OrderedSet<int>.New(new int[] { 1 });
			Assert.That(set2.Length, Is.EqualTo(1));
			Assert.That(((ICollection)set2).Count, Is.EqualTo(1));

			var set3 = OrderedSet<int>.New(new int[] { 1, 2, 2, 4 });
			Assert.That(set3.Length, Is.EqualTo(3));
			Assert.That(((ICollection)set3).Count, Is.EqualTo(3));
		}

		[Test]
		public void Union()
		{
			var a = OrderedSet<int>.New(new int[] { 1, 2 });
			var b = OrderedSet<int>.New(new int[] { 2, 3 });
			var x = a.Union(b);

			Assert.That(x.ContainsValue(1), Is.EqualTo(true), "1");
			Assert.That(x.ContainsValue(2), Is.EqualTo(true), "2");
			Assert.That(x.ContainsValue(3), Is.EqualTo(true), "3");
			Assert.That(x.ContainsValue(4), Is.EqualTo(false), "4");
		}

		[Test]
		public void Intersect()
		{
			var a = OrderedSet<int>.New(new int[] { 1, 2 });
			var b = OrderedSet<int>.New(new int[] { 2, 3 });
			var x = a.Intersection(b);

			Assert.That(x.ContainsValue(1), Is.EqualTo(false), "1");
			Assert.That(x.ContainsValue(2), Is.EqualTo(true), "2");
			Assert.That(x.ContainsValue(3), Is.EqualTo(false), "3");
			Assert.That(x.ContainsValue(4), Is.EqualTo(false), "4");
		}

		[Test]
		public void Subtract()
		{
			var a = OrderedSet<int>.New(new int[] { 1, 2 });
			var b = OrderedSet<int>.New(new int[] { 2, 3 });
			var x = a.Subtract(b);

			Assert.That(x.ContainsValue(1), Is.EqualTo(true), "1");
			Assert.That(x.ContainsValue(2), Is.EqualTo(false), "2");
			Assert.That(x.ContainsValue(3), Is.EqualTo(false), "3");
			Assert.That(x.ContainsValue(4), Is.EqualTo(false), "4");
		}

		[Test]
		public void IsOverlapping()
		{
			var set1 = OrderedSet<int>.New(new int[] { 1 });
			var set2 = OrderedSet<int>.New(new int[] { 1, 2 });
			var set3 = OrderedSet<int>.New(new int[] { 2, 3, 4 });

			Assert.That(set1.Intersects(set2), Is.EqualTo(true));
			Assert.That(set1.Intersects(set3), Is.EqualTo(false));

			Assert.That(set2.Intersects(set1), Is.EqualTo(true));
			Assert.That(set2.Intersects(set3), Is.EqualTo(true));

			Assert.That(set3.Intersects(set1), Is.EqualTo(false));
			Assert.That(set3.Intersects(set2), Is.EqualTo(true));
		}

		[Test]
		public void IsSuperSetOf()
		{
			var set1 = OrderedSet<int>.New(new int[] { 1 });
			var set2 = OrderedSet<int>.New(new int[] { 1, 2 });
			var set3 = OrderedSet<int>.New(new int[] { 2, 3, 4 });

			Assert.That(set1.IsSupersetOf(set2), Is.EqualTo(false));
			Assert.That(set1.IsSupersetOf(set3), Is.EqualTo(false));

			Assert.That(set2.IsSupersetOf(set1), Is.EqualTo(true));
			Assert.That(set2.IsSupersetOf(set3), Is.EqualTo(false));

			Assert.That(set3.IsSupersetOf(set1), Is.EqualTo(false));
			Assert.That(set3.IsSupersetOf(set2), Is.EqualTo(false));
		}

		[Test]
		public void CopyTo()
		{
			var set = OrderedSet<int>.New(new int[] { 3, 4, 1 });
			var target = new int[5];
			set.CopyTo(target, 1);

			Assert.That(target, Is.EquivalentTo(new int[] { 0, 1, 3, 4, 0 }));
		}
	}
}
