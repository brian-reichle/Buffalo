// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class CharSetTest
	{
		[Test]
		public void New()
		{
			Assert.That(CharSet.New('A').ToString(), Is.EqualTo("[A]"));

			Assert.That(CharSet.New(System.Array.Empty<CharRange>()).ToString(), Is.EqualTo("[]"));
			Assert.That(CharSet.New(new CharRange[] { new CharRange('A', 'Z') }).ToString(), Is.EqualTo("[A-Z]"));
			Assert.That(CharSet.New(new CharRange[] { new CharRange('N', 'Z'), new CharRange('A', 'L') }).ToString(), Is.EqualTo("[A-L,N-Z]"));
			Assert.That(CharSet.New(new CharRange[] { new CharRange('N', 'Z'), new CharRange('A', 'M') }).ToString(), Is.EqualTo("[A-Z]"));
			Assert.That(CharSet.New(new CharRange[] { new CharRange('N', 'Z'), new CharRange('A', 'N') }).ToString(), Is.EqualTo("[A-Z]"));
			Assert.That(CharSet.New(new CharRange[] { new CharRange('N', 'Z'), new CharRange('A', 'O') }).ToString(), Is.EqualTo("[A-Z]"));
		}

		[Test]
		public void Union()
		{
			var abc = new CharRange[] { new CharRange('A', 'C') };
			var abd = new CharRange[] { new CharRange('A', 'B'), new CharRange('D', 'D') };
			var acd = new CharRange[] { new CharRange('A', 'A'), new CharRange('C', 'D') };
			var bcd = new CharRange[] { new CharRange('B', 'D') };

			var set1 = CharSet.New(abc);
			var set2 = CharSet.New(abd);
			var set3 = CharSet.Universal.Subtract(CharSet.New(acd));
			var set4 = CharSet.Universal.Subtract(CharSet.New(bcd));

			Assert.That(set1.Union(set2).ToString(), Is.EqualTo("[A-D]"));
			Assert.That(set2.Union(set3).ToString(), Is.EqualTo("![C]"));
			Assert.That(set3.Union(set4).ToString(), Is.EqualTo("![C-D]"));
			Assert.That(set4.Union(set1).ToString(), Is.EqualTo("![D]"));
		}

		[Test]
		public void Union2()
		{
			/*
			 * A    : ABCD..GHIJKLMN....
			 * B    : ..CDEFGH..KL....QR
			 * A u B: ABCDEFGHIJKLMN..QR
			 * B u A: ABCDEFGHIJKLMN..QR
			 */

			var a = CharSet.New(new CharRange[]
			{
				new CharRange('A', 'D'),
				new CharRange('G', 'N'),
			});

			var b = CharSet.New(new CharRange[]
			{
				new CharRange('C', 'H'),
				new CharRange('K', 'L'),
				new CharRange('Q', 'R'),
			});

			Assert.That(a.Union(b).ToString(), Is.EqualTo("[A-N,Q-R]"), "A union B");
			Assert.That(b.Union(a).ToString(), Is.EqualTo("[A-N,Q-R]"), "B union A");
		}

		[Test]
		public void Intersect()
		{
			var abc = new CharRange[] { new CharRange('A', 'C') };
			var abd = new CharRange[] { new CharRange('A', 'B'), new CharRange('D', 'D') };
			var acd = new CharRange[] { new CharRange('A', 'A'), new CharRange('C', 'D') };
			var bcd = new CharRange[] { new CharRange('B', 'D') };

			var set1 = CharSet.New(abc);
			var set2 = CharSet.New(abd);
			var set3 = CharSet.Universal.Subtract(CharSet.New(acd));
			var set4 = CharSet.Universal.Subtract(CharSet.New(bcd));

			Assert.That(set1.Intersection(set2).ToString(), Is.EqualTo("[A-B]"));
			Assert.That(set2.Intersection(set3).ToString(), Is.EqualTo("[B]"));
			Assert.That(set3.Intersection(set4).ToString(), Is.EqualTo("![A-D]"));
			Assert.That(set4.Intersection(set1).ToString(), Is.EqualTo("[A]"));
		}

		[Test]
		public void Intersect2()
		{
			/*
			 * A    : ABCD..GHIJKLMN....
			 * B    : ..CDEFGH..KL....QR
			 * A i B: ..CD..GH..KL......
			 * B i A: ..CD..GH..KL......
			 */

			var a = CharSet.New(new CharRange[]
			{
				new CharRange('A', 'D'),
				new CharRange('G', 'N'),
			});

			var b = CharSet.New(new CharRange[]
			{
				new CharRange('C', 'H'),
				new CharRange('K', 'L'),
				new CharRange('Q', 'R'),
			});

			Assert.That(a.Intersection(b).ToString(), Is.EqualTo("[C-D,G-H,K-L]"), "A intersect B");
			Assert.That(b.Intersection(a).ToString(), Is.EqualTo("[C-D,G-H,K-L]"), "B intersect A");
		}

		[Test]
		public void Subtract()
		{
			var abc = new CharRange[] { new CharRange('A', 'C') };
			var abd = new CharRange[] { new CharRange('A', 'B'), new CharRange('D', 'D') };
			var acd = new CharRange[] { new CharRange('A', 'A'), new CharRange('C', 'D') };
			var bcd = new CharRange[] { new CharRange('B', 'D') };

			var set1 = CharSet.New(abc);
			var set2 = CharSet.New(abd);
			var set3 = CharSet.Universal.Subtract(CharSet.New(acd));
			var set4 = CharSet.Universal.Subtract(CharSet.New(bcd));

			Assert.That(set1.Subtract(set2).ToString(), Is.EqualTo("[C]"));
			Assert.That(set2.Subtract(set3).ToString(), Is.EqualTo("[A,D]"));
			Assert.That(set3.Subtract(set4).ToString(), Is.EqualTo("[B]"));
			Assert.That(set4.Subtract(set1).ToString(), Is.EqualTo("![A-D]"));
		}

		[Test]
		public void Subtract2()
		{
			/*
			 * A    : ABCD..GHIJKLMN....
			 * B    : ..CDEFGH..KL....QR
			 * A - B: AB......IJ..MN....
			 * B - A: ....EF..........QR
			 */

			var a = CharSet.New(new CharRange[]
			{
				new CharRange('A', 'D'),
				new CharRange('G', 'N'),
			});

			var b = CharSet.New(new CharRange[]
			{
				new CharRange('C', 'H'),
				new CharRange('K', 'L'),
				new CharRange('Q', 'R'),
			});

			Assert.That(a.Subtract(b).ToString(), Is.EqualTo("[A-B,I-J,M-N]"), "A subtract B");
			Assert.That(b.Subtract(a).ToString(), Is.EqualTo("[E-F,Q-R]"), "B subtract A");
		}

		[Test]
		public void Contains()
		{
			var set1 = CharSet.New(new CharRange[] { new CharRange('A', 'D') });
			Assert.That(set1.ContainsChar('A'), Is.EqualTo(true));
			Assert.That(set1.ContainsChar('B'), Is.EqualTo(true));
			Assert.That(set1.ContainsChar('C'), Is.EqualTo(true));
			Assert.That(set1.ContainsChar('D'), Is.EqualTo(true));
			Assert.That(set1.ContainsChar('E'), Is.EqualTo(false));
		}

		[Test]
		public void Intersects()
		{
			var bc = new CharRange[] { new CharRange('B', 'C') };
			var cd = new CharRange[] { new CharRange('C', 'D') };
			var de = new CharRange[] { new CharRange('D', 'E') };

			Assert.That(CharSet.New(bc).Intersects(CharSet.New(bc)), Is.EqualTo(true));
			Assert.That(CharSet.New(bc).Intersects(CharSet.New(cd)), Is.EqualTo(true));
			Assert.That(CharSet.New(bc).Intersects(CharSet.New(de)), Is.EqualTo(false));
		}

		[Test]
		public void IsSuperSetOf()
		{
			var abcd = new CharRange[] { new CharRange('A', 'D') };
			var abc = new CharRange[] { new CharRange('A', 'C') };
			var bc = new CharRange[] { new CharRange('B', 'C') };
			var cd = new CharRange[] { new CharRange('C', 'D') };
			var de = new CharRange[] { new CharRange('D', 'E') };

			Assert.That(CharSet.New(abc).IsSupersetOf(CharSet.New(abcd)), Is.EqualTo(false));
			Assert.That(CharSet.New(abc).IsSupersetOf(CharSet.New(abc)), Is.EqualTo(true));
			Assert.That(CharSet.New(abc).IsSupersetOf(CharSet.New(bc)), Is.EqualTo(true));
			Assert.That(CharSet.New(abc).IsSupersetOf(CharSet.New(cd)), Is.EqualTo(false));
			Assert.That(CharSet.New(abc).IsSupersetOf(CharSet.New(de)), Is.EqualTo(false));
		}
	}
}
