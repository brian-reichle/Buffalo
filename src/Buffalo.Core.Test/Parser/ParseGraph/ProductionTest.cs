// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Immutable;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class ProductionTest
	{
		[Test]
		public void New()
		{
			var segment0 = new Segment("E", false);
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var production = new Production(segment0, ImmutableArray.Create(segment1, segment2, segment3));

			Assert.That(production.Target, Is.SameAs(segment0));
			Assert.That(production.Segments.Length, Is.EqualTo(3));
			Assert.That(production.Segments[0], Is.SameAs(segment1));
			Assert.That(production.Segments[1], Is.SameAs(segment2));
			Assert.That(production.Segments[2], Is.SameAs(segment3));
		}

		[Test]
		public void ToStringImplementation()
		{
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var production = new Production(segment1, ImmutableArray.Create(segment1, segment2, segment3));

			Assert.That(production.ToString(), Is.EqualTo("<E> -> <E> + Number"));
		}

		[Test]
		public void Equality()
		{
			var segment0 = new Segment("E", false);
			var segment1 = new Segment("E", false);
			var segment2 = new Segment("+", true);
			var segment3 = new Segment("Number", true);

			var productionA = new Production(segment0, ImmutableArray.Create(segment1, segment2, segment3));
			var productionB = new Production(segment1, ImmutableArray.Create(segment0, segment2, segment3));
			var productionC = new Production(segment2, ImmutableArray.Create(segment1, segment2, segment2));
			var productionD = new Production(segment0, ImmutableArray.Create(segment1, segment1, segment2));
			var productionE = new Production(segment0, ImmutableArray.Create(segment1, segment2));

			Assert.That(productionA.Equals(productionB), Is.EqualTo(true), "productionA == productionB");
			Assert.That(productionB.Equals(productionA), Is.EqualTo(true), "productionB == productionA");
			Assert.That(productionA.GetHashCode() == productionB.GetHashCode(), Is.EqualTo(true), "productionA.HC == productionB.HC");

			Assert.That(productionA.Equals(productionC), Is.EqualTo(false), "productionA == productionC");
			Assert.That(productionC.Equals(productionA), Is.EqualTo(false), "productionC == productionA");
			Assert.That(productionA.GetHashCode() == productionC.GetHashCode(), Is.EqualTo(false), "productionA.HC == productionC.HC");

			Assert.That(productionA.Equals(productionD), Is.EqualTo(false), "productionA == productionD");
			Assert.That(productionD.Equals(productionA), Is.EqualTo(false), "productionD == productionA");
			Assert.That(productionA.GetHashCode() == productionD.GetHashCode(), Is.EqualTo(false), "productionA.HC == productionD.HC");

			Assert.That(productionA.Equals(productionE), Is.EqualTo(false), "productionA == productionE");
			Assert.That(productionE.Equals(productionA), Is.EqualTo(false), "productionE == productionA");
			Assert.That(productionA.GetHashCode() == productionE.GetHashCode(), Is.EqualTo(false), "productionA.HC == productionE.HC");
		}
	}
}
