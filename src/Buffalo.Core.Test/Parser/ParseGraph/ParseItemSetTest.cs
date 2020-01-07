// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Immutable;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public class ParseItemSetTest
	{
		[Test]
		public void New()
		{
			var set1 = ParseItemSet.New(new ParseItem[] { _item10, _item10, _item11, _item20 });
			var set2 = ParseItemSet.New(new ParseItem[] { _item10, _item11, _item20 });
			var set3 = ParseItemSet.New(new ParseItem[] { _item10 });

			Assert.That(set1.Equals(set1), Is.EqualTo(true));
			Assert.That(set1.Equals(set2), Is.EqualTo(true));
			Assert.That(set1.Equals(set3), Is.EqualTo(false));

			Assert.That(set2.Equals(set1), Is.EqualTo(true));
			Assert.That(set2.Equals(set2), Is.EqualTo(true));
			Assert.That(set2.Equals(set3), Is.EqualTo(false));

			Assert.That(set3.Equals(set1), Is.EqualTo(false));
			Assert.That(set3.Equals(set2), Is.EqualTo(false));
			Assert.That(set3.Equals(set3), Is.EqualTo(true));
		}

		[Test]
		public void SetLookahead()
		{
			var set1 = ParseItemSet.New(new ParseItem[] { _item10, _item11, _item20 });

			var la1 = SegmentSet.New(new Segment[] { _lookaheadA, _lookaheadB });
			var la2 = SegmentSet.New(new Segment[] { _lookaheadB, _lookaheadC });

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(SegmentSet.EmptySet));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(SegmentSet.EmptySet));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));

			set1.SetLookahead(_item10, la1);
			set1.SetLookahead(_item11, la1);

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));

			set1.SetLookahead(_item10, la2);

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(la2));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));
		}

		[Test]
		public void TryUnionLookahead()
		{
			var set1 = ParseItemSet.New(new ParseItem[] { _item10, _item11, _item20 });

			var la1 = SegmentSet.New(new Segment[] { _lookaheadA, _lookaheadB });
			var la2 = SegmentSet.New(new Segment[] { _lookaheadB, _lookaheadC });
			var la3 = SegmentSet.New(new Segment[] { _lookaheadA, _lookaheadB, _lookaheadC });

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(SegmentSet.EmptySet));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(SegmentSet.EmptySet));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));

			Assert.That(set1.TryUnionLookahead(_item10, la1), Is.EqualTo(true));
			Assert.That(set1.TryUnionLookahead(_item11, la1), Is.EqualTo(true));

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));

			Assert.That(set1.TryUnionLookahead(_item10, la2), Is.EqualTo(true));

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(la3));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));

			Assert.That(set1.TryUnionLookahead(_item10, la1), Is.EqualTo(false));

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(la3));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(la1));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(SegmentSet.EmptySet));
		}

		[Test]
		public void TrySubtractLookahead()
		{
			var set1 = ParseItemSet.New(new ParseItem[] { _item10, _item11, _item20 });

			var laA = SegmentSet.New(new Segment[] { _lookaheadA });
			var laB = SegmentSet.New(new Segment[] { _lookaheadB });
			var laC = SegmentSet.New(new Segment[] { _lookaheadC });

			var laAB = SegmentSet.New(new Segment[] { _lookaheadA, _lookaheadB });
			var laBC = SegmentSet.New(new Segment[] { _lookaheadB, _lookaheadC });
			var laABC = SegmentSet.New(new Segment[] { _lookaheadA, _lookaheadB, _lookaheadC });

			set1.SetLookahead(_item10, laAB);
			set1.SetLookahead(_item11, laBC);
			set1.SetLookahead(_item20, laABC);

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(laAB));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(laBC));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(laABC));

			set1.SubtractLookaheads(laB);

			Assert.That(set1.GetLookahead(_item10), Is.EqualTo(laA));
			Assert.That(set1.GetLookahead(_item11), Is.EqualTo(laC));
			Assert.That(set1.GetLookahead(_item20), Is.EqualTo(laA.Union(laC)));
		}

		[Test]
		public void Contains()
		{
			var set = ParseItemSet.New(new ParseItem[] { _item10, _item10, _item11, _item11, _item20 });

			Assert.That(set.Contains(_item10), Is.EqualTo(true));
			Assert.That(set.Contains(_item11), Is.EqualTo(true));
			Assert.That(set.Contains(_item20), Is.EqualTo(true));
			Assert.That(set.Contains(_item21), Is.EqualTo(false));
		}

		[Test]
		public void Equality()
		{
			var set1 = ParseItemSet.New(new ParseItem[]
			{
				_item10,
				_item10,
				_item10,
			});

			var set2 = ParseItemSet.New(new ParseItem[]
			{
				_item10,
			});

			var set3 = ParseItemSet.New(new ParseItem[]
			{
				_item10,
				_item11,
			});

			Assert.That(set1.Equals(set1), Is.EqualTo(true));
			Assert.That(set1.Equals(set2), Is.EqualTo(true));
			Assert.That(set1.Equals(set3), Is.EqualTo(false));

			Assert.That(set2.Equals(set1), Is.EqualTo(true));
			Assert.That(set2.Equals(set2), Is.EqualTo(true));
			Assert.That(set2.Equals(set3), Is.EqualTo(false));

			Assert.That(set3.Equals(set1), Is.EqualTo(false));
			Assert.That(set3.Equals(set2), Is.EqualTo(false));
			Assert.That(set3.Equals(set3), Is.EqualTo(true));

			Assert.That(set1.GetHashCode(), Is.EqualTo(set1.GetHashCode()));
			Assert.That(set1.GetHashCode(), Is.EqualTo(set2.GetHashCode()));
			Assert.That(set1.GetHashCode(), Is.Not.EqualTo(set3.GetHashCode()));

			Assert.That(set2.GetHashCode(), Is.EqualTo(set1.GetHashCode()));
			Assert.That(set2.GetHashCode(), Is.EqualTo(set2.GetHashCode()));
			Assert.That(set2.GetHashCode(), Is.Not.EqualTo(set3.GetHashCode()));

			Assert.That(set3.GetHashCode(), Is.Not.EqualTo(set1.GetHashCode()));
			Assert.That(set3.GetHashCode(), Is.Not.EqualTo(set2.GetHashCode()));
			Assert.That(set3.GetHashCode(), Is.EqualTo(set3.GetHashCode()));
		}

		[Test]
		public void GetTransitionKernels()
		{
			var set1 = ParseItemSet.New(new ParseItem[] { _item10, _item11, _item20, _item21 });

			var transitions = set1.GetTransitionKernels();

			Assert.That(transitions.Count, Is.EqualTo(2));
			Assert.That(transitions[_segment1], Is.EquivalentTo(new ParseItem[] { _item11 }));
			Assert.That(transitions[_segment2], Is.EquivalentTo(new ParseItem[] { _item21 }));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			_target1 = new Segment("target1", false);
			_target2 = new Segment("target2", false);
			_segment1 = new Segment("segment1", true);
			_segment2 = new Segment("segment2", true);
			_lookaheadA = new Segment("lookaheadA", true);
			_lookaheadB = new Segment("lookaheadB", true);
			_lookaheadC = new Segment("lookaheadC", true);

			_production1 = new Production(_target1, ImmutableArray.Create(_segment1));
			_production2 = new Production(_target2, ImmutableArray.Create(_segment2));

			_item10 = new ParseItem(_production1, 0);
			_item11 = new ParseItem(_production1, 1);
			_item20 = new ParseItem(_production2, 0);
			_item21 = new ParseItem(_production2, 1);
		}

		Segment _target1;
		Segment _target2;
		Segment _segment1;
		Segment _segment2;
		Segment _lookaheadA;
		Segment _lookaheadB;
		Segment _lookaheadC;

		Production _production1;
		Production _production2;

		ParseItem _item10;
		ParseItem _item11;
		ParseItem _item20;
		ParseItem _item21;
	}
}
