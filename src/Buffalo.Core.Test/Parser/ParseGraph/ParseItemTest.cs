// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Immutable;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class ParseItemTest
	{
		[Test]
		public void New()
		{
			var segment0 = new Segment("E", false);
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var production = new Production(segment0, ImmutableArray.Create(segment1, segment2, segment3));

			var item = new ParseItem(production, 1);

			Assert.That(item.Production, Is.SameAs(production));
			Assert.That(item.Position, Is.EqualTo(1));
		}

		[Test]
		public void NextItem()
		{
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var production = new Production(segment1, ImmutableArray.Create(segment1, segment2, segment3));

			var item0 = new ParseItem(production, 0);
			var item1 = new ParseItem(production, 1);
			var item2 = new ParseItem(production, 2);
			var item3 = new ParseItem(production, 3);

			Assert.That(item0.NextItem(), Is.EqualTo(item1));
			Assert.That(item1.NextItem(), Is.EqualTo(item2));
			Assert.That(item2.NextItem(), Is.EqualTo(item3));
			Assert.That(item3.NextItem(), Is.EqualTo(null));
		}

		[Test]
		public void ToStringImplementation()
		{
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var production = new Production(segment1, ImmutableArray.Create(segment1, segment2, segment3));

			var item0 = new ParseItem(production, 0);
			var item1 = new ParseItem(production, 1);
			var item2 = new ParseItem(production, 2);
			var item3 = new ParseItem(production, 3);

			Assert.That(item0.ToString(), Is.EqualTo("[<E> -> \u2022 <E> + Number]"));
			Assert.That(item1.ToString(), Is.EqualTo("[<E> -> <E> \u2022 + Number]"));
			Assert.That(item2.ToString(), Is.EqualTo("[<E> -> <E> + \u2022 Number]"));
			Assert.That(item3.ToString(), Is.EqualTo("[<E> -> <E> + Number \u2022]"));
		}

		[Test]
		public void Equality()
		{
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var production1 = new Production(segment1, ImmutableArray.Create(segment1, segment2, segment3));
			var production2 = new Production(segment1, ImmutableArray.Create(segment1, segment2, segment3));
			var production3 = new Production(segment1, ImmutableArray.Create(segment3));

			var itemA = new ParseItem(production1, 1);
			var itemB = new ParseItem(production2, 1);
			var itemC = new ParseItem(production3, 1);
			var itemD = new ParseItem(production1, 2);

			Assert.That(itemA.Equals(itemB), Is.EqualTo(true), "itemA == itemB");
			Assert.That(itemB.Equals(itemA), Is.EqualTo(true), "itemB == itemA");
			Assert.That(itemA.GetHashCode() == itemB.GetHashCode(), Is.EqualTo(true), "itemA.HC == itemB.HC");

			Assert.That(itemA.Equals(itemC), Is.EqualTo(false), "itemA == itemC");
			Assert.That(itemC.Equals(itemA), Is.EqualTo(false), "itemC == itemA");
			Assert.That(itemA.GetHashCode() == itemC.GetHashCode(), Is.EqualTo(false), "itemA.HC == itemC.HC");

			Assert.That(itemA.Equals(itemD), Is.EqualTo(false), "itemA == itemD");
			Assert.That(itemD.Equals(itemA), Is.EqualTo(false), "itemD == itemA");
			Assert.That(itemA.GetHashCode() == itemD.GetHashCode(), Is.EqualTo(false), "itemA.HC == itemD.HC");
		}
	}
}
