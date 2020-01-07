// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class SegmentTest
	{
		[Test]
		public void StaticSegments()
		{
			Assert.That(Segment.EOF.ToString(), Is.EqualTo("EOF"));
			Assert.That(Segment.Error.ToString(), Is.EqualTo("Error"));
		}

		[Test]
		public void Equality()
		{
			var segment1 = new Segment("Bob", true);
			var segment2 = new Segment("Bob", true);
			var segment3 = new Segment("Fread", true);
			var segment4 = new Segment("Bob", false);
			var segment5 = new Segment("Aob", false);

			var segment1o = segment1.GetOptional();
			var segment2o = segment2.GetOptional();
			var segment3o = segment3.GetOptional();
			var segment4o = segment4.GetOptional();
			var segment5o = segment5.GetOptional();

			var segment4i = segment4.GetInitial();
			var segment5i = segment5.GetInitial();

			Segment[] segmentOrder =
			{
				segment4i,
				segment5i,
				segment1o,
				segment3o,
				segment4o,
				segment5o,
				segment1,
				segment3,
				segment4,
				segment5,
			};

			for (var i = 0; i < segmentOrder.Length; i++)
			{
				var s1 = segmentOrder[i];

				for (var j = i + 1; j < segmentOrder.Length; j++)
				{
					var s2 = segmentOrder[j];

					Assert.That(s1.Equals(s2), Is.False, "{0} != {1}", s1, s2);
					Assert.That(s2.Equals(s1), Is.False, "{0} != {1}", s2, s1);
					Assert.That(s1.GetHashCode() == s2.GetHashCode(), Is.False, "s1.HC != s2.HC");
				}
			}

			Assert.That(segment1.Equals(segment2), Is.True, "segment1 == segment2");
			Assert.That(segment2.Equals(segment1), Is.True, "segment2 == segment1");
			Assert.That(segment1.GetHashCode() == segment2.GetHashCode(), Is.True, "segment1.HC == segment2.HC");

			Assert.That(segment1o.Equals(segment2o), Is.True, "segment1o == segment2o");
			Assert.That(segment2o.Equals(segment1o), Is.True, "segment2o == segment1o");
			Assert.That(segment1o.GetHashCode() == segment2o.GetHashCode(), Is.True, "segment1o.HC == segment2o.HC");
		}

		[Test]
		public void Compare()
		{
			var segment1 = new Segment("Bob", true);
			var segment2 = new Segment("Bob", true);
			var segment3 = new Segment("Fread", true);
			var segment4 = new Segment("Bob", false);

			var segment1o = segment1.GetOptional();
			var segment2o = segment2.GetOptional();
			var segment3o = segment3.GetOptional();
			var segment4o = segment4.GetOptional();

			var segment4i = segment4.GetInitial();

			Segment[] segmentOrder =
			{
				segment4i,
				segment1o,
				segment3o,
				segment4o,
				segment1,
				segment3,
				segment4,
			};

			for (var i = 0; i < segmentOrder.Length; i++)
			{
				var s1 = segmentOrder[i];

				for (var j = i + 1; j < segmentOrder.Length; j++)
				{
					var s2 = segmentOrder[j];

					Assert.That(s1.CompareTo(s2), Is.LessThan(0), "{0} < {1}", s1, s2);
					Assert.That(s2.CompareTo(s1), Is.GreaterThan(0), "{0} > {1}", s2, s1);
				}
			}

			Assert.That(segment1.CompareTo(segment1), Is.EqualTo(0));
			Assert.That(segment1o.CompareTo(segment2o), Is.EqualTo(0));
		}

		[Test]
		public void ToStringOverride()
		{
			Assert.That(new Segment("A", true).ToString(), Is.EqualTo("A"));
			Assert.That(new Segment("A", false).ToString(), Is.EqualTo("<A>"));
			Assert.That(new Segment("A", true).GetOptional().ToString(), Is.EqualTo("A?"));
			Assert.That(new Segment("A", false).GetOptional().ToString(), Is.EqualTo("<A>?"));
			Assert.That(new Segment("A", false).GetInitial().ToString(), Is.EqualTo("<A'>"));
		}
	}
}
