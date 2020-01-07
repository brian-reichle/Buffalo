// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class SegmentSetTest
	{
		[Test]
		public void New()
		{
			var segment1 = new Segment("segment1", true);
			var segment2 = new Segment("segment2", true);

			var set1 = SegmentSet.New(new Segment[] { segment1, segment2, segment2 });
			var set2 = SegmentSet.New(new Segment[] { segment2, segment1 });
			var set3 = SegmentSet.New(new Segment[] { segment2, segment2 });

			Assert.That(set1.Equals(set2), Is.EqualTo(true));
			Assert.That(set2.Equals(set1), Is.EqualTo(true));

			Assert.That(set1.Equals(set3), Is.EqualTo(false));
			Assert.That(set3.Equals(set1), Is.EqualTo(false));

			Assert.That(set2.Equals(set3), Is.EqualTo(false));
			Assert.That(set3.Equals(set2), Is.EqualTo(false));
		}

		[Test]
		public void ToStringX()
		{
			var set1 = SegmentSet.New(new Segment[] { new Segment("a", true) });
			var set2 = SegmentSet.New(new Segment[] { new Segment("A", false) });

			Assert.That(SegmentSet.EmptySet.ToString(), Is.EqualTo(string.Empty));

			Assert.That(SegmentSet.EpsilonSet.ToString(), Is.EqualTo("null"));
			Assert.That(set1.ToString(), Is.EqualTo("a"));
			Assert.That(set2.ToString(), Is.EqualTo("<A>"));

			Assert.That(set1.Union(set2).Union(SegmentSet.EpsilonSet).ToString(), Is.EqualTo("null a <A>"));
		}

		[Test]
		public void Union()
		{
			var segment1 = new Segment("Segment1", false);
			var segment2 = new Segment("Segment2", false);
			var segment3 = new Segment("Segment3", false);
			var segment4 = new Segment("Segment4", false);

			var a = SegmentSet.New(new Segment[] { segment1, segment2 });
			var b = SegmentSet.New(new Segment[] { segment2, segment3 });
			var x = a.Union(b);

			Assert.That(x.ContainsSegment(segment1), Is.EqualTo(true), "segment1");
			Assert.That(x.ContainsSegment(segment2), Is.EqualTo(true), "segment2");
			Assert.That(x.ContainsSegment(segment3), Is.EqualTo(true), "segment3");
			Assert.That(x.ContainsSegment(segment4), Is.EqualTo(false), "segment4");
		}

		[Test]
		public void Union_Reuse()
		{
			var segment1 = new Segment("Segment1", false);
			var segment2 = new Segment("Segment2", false);

			var set1 = SegmentSet.New(new Segment[] { segment1, segment2 });
			var set2 = SegmentSet.New(new Segment[] { segment1 });

			Assert.That(set1.Union(set2), Is.SameAs(set1));
			Assert.That(set2.Union(set1), Is.SameAs(set1));
		}

		[Test]
		public void Intersect()
		{
			var segment1 = new Segment("segment1", false);
			var segment2 = new Segment("segment2", false);
			var segment3 = new Segment("segment3", false);
			var segment4 = new Segment("segment4", false);

			var a = SegmentSet.New(new Segment[] { segment1, segment2 });
			var b = SegmentSet.New(new Segment[] { segment2, segment3 });
			var x = a.Intersection(b);

			Assert.That(x.ContainsSegment(segment1), Is.EqualTo(false), "segment1");
			Assert.That(x.ContainsSegment(segment2), Is.EqualTo(true), "segment2");
			Assert.That(x.ContainsSegment(segment3), Is.EqualTo(false), "segment3");
			Assert.That(x.ContainsSegment(segment4), Is.EqualTo(false), "segment4");
		}

		[Test]
		public void Intersect_Reuse()
		{
			var segment1 = new Segment("Segment1", false);
			var segment2 = new Segment("Segment2", false);

			var set1 = SegmentSet.New(new Segment[] { segment1, segment2 });
			var set2 = SegmentSet.New(new Segment[] { segment1 });

			Assert.That(set1.Intersection(set2), Is.SameAs(set2));
			Assert.That(set2.Intersection(set1), Is.SameAs(set2));
		}

		[Test]
		public void Subtract()
		{
			var segment1 = new Segment("segment1", false);
			var segment2 = new Segment("segment2", false);
			var segment3 = new Segment("segment3", false);
			var segment4 = new Segment("segment4", false);

			var a = SegmentSet.New(new Segment[] { segment1, segment2 });
			var b = SegmentSet.New(new Segment[] { segment2, segment3 });
			var x = a.Subtract(b);

			Assert.That(x.ContainsSegment(segment1), Is.EqualTo(true), "segment1");
			Assert.That(x.ContainsSegment(segment2), Is.EqualTo(false), "segment2");
			Assert.That(x.ContainsSegment(segment3), Is.EqualTo(false), "segment3");
			Assert.That(x.ContainsSegment(segment4), Is.EqualTo(false), "segment4");
		}

		[Test]
		public void Subtract_Reuse()
		{
			var segment1 = new Segment("Segment1", false);
			var segment2 = new Segment("Segment2", false);

			var set1 = SegmentSet.New(new Segment[] { segment1 });
			var set2 = SegmentSet.New(new Segment[] { segment2 });

			Assert.That(set1.Subtract(set2), Is.SameAs(set1));
			Assert.That(set2.Subtract(set1), Is.SameAs(set2));
		}

		[Test]
		public void IsOverlapping()
		{
			var segment1 = new Segment("segment1", false);
			var segment2 = new Segment("segment2", false);
			var segment3 = new Segment("segment3", false);
			var segment4 = new Segment("segment4", false);

			var set1 = SegmentSet.New(new Segment[] { segment1 });
			var set2 = SegmentSet.New(new Segment[] { segment1, segment2 });
			var set3 = SegmentSet.New(new Segment[] { segment2, segment3, segment4 });

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
			var segment1 = new Segment("segment1", false);
			var segment2 = new Segment("segment2", false);
			var segment3 = new Segment("segment3", false);
			var segment4 = new Segment("segment4", false);

			var set1 = SegmentSet.New(new Segment[] { segment1 });
			var set2 = SegmentSet.New(new Segment[] { segment1, segment2 });
			var set3 = SegmentSet.New(new Segment[] { segment2, segment3, segment4 });

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
			var segment1 = new Segment("segment1", false);
			var segment2 = new Segment("segment2", false);
			var segment3 = new Segment("segment3", false);

			var set = SegmentSet.New(new Segment[] { segment1, segment2, segment3 });

			var segments1 = new Segment[set.Count];
			set.CopyTo(segments1, 0);
			Assert.That(segments1, Is.EqualTo(new Segment[] { segment1, segment2, segment3 }));

			var segments2 = new Segment[set.Count + 2];
			set.CopyTo(segments2, 1);
			Assert.That(segments2, Is.EqualTo(new Segment[] { null, segment1, segment2, segment3, null }));
		}

		[Test]
		public void Modify()
		{
			var segment1 = new Segment("segment1", false);
			var segment2 = new Segment("segment2", false);

			ICollection<Segment> set = SegmentSet.New(new Segment[] { segment1 });
			Assert.That(set.IsReadOnly, Is.EqualTo(true));

			try
			{
				set.Add(segment2);
				Assert.Fail("Add should have thrown an exception.");
			}
			catch (NotSupportedException)
			{
			}

			try
			{
				set.Remove(segment1);
				Assert.Fail("Remove should have thrown an exception.");
			}
			catch (NotSupportedException)
			{
			}
		}
	}
}
