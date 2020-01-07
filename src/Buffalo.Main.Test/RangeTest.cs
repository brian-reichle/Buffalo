// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using NUnit.Framework;

namespace Buffalo.Main.Test
{
	[TestFixture]
	public class RangeTest
	{
		[Test]
		public void Equality()
		{
			var r1 = new Range(10, 10);
			var r2 = new Range(10, 10);
			var r3 = new Range(10, 11);
			var r4 = new Range(11, 10);

			Assert.That(r1.Equals(r2), Is.EqualTo(true));
			Assert.That(r2.Equals(r1), Is.EqualTo(true));
			Assert.That(r1.GetHashCode(), Is.EqualTo(r2.GetHashCode()));

			Assert.That(r1.Equals(r3), Is.EqualTo(false));
			Assert.That(r3.Equals(r1), Is.EqualTo(false));
			Assert.That(r1.GetHashCode(), Is.Not.EqualTo(r3.GetHashCode()));

			Assert.That(r1.Equals(r4), Is.EqualTo(false));
			Assert.That(r4.Equals(r1), Is.EqualTo(false));
			Assert.That(r1.GetHashCode(), Is.Not.EqualTo(r4.GetHashCode()));
		}

		[Test]
		public void ToStringOverride()
		{
			Assert.That(new Range(10, 5).ToString(), Is.EqualTo("(10)-(14)"));
			Assert.That(new Range(1, 2).ToString(), Is.EqualTo("(1)-(2)"));
		}
	}
}
