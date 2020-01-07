// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class SegmentSetProviderTest
	{
		[Test]
		public void GetFirsts()
		{
			string Converter(SegmentSetProvider provider)
			{
				var builder = new StringBuilder();

				foreach (var nonTerminal in _nonTerminals)
				{
					var set = provider.GetFirstsSet(nonTerminal);
					if (set.Count == 0) continue;

					nonTerminal.AppendTo(builder);
					builder.Append(": ");
					set.AppendTo(builder);
					builder.AppendLine();
				}

				return builder.ToString();
			}

			const string expected1 =
				"<S'>: if other\r\n" +
				"<I>: if\r\n" +
				"<S>: if other\r\n" +
				"";

			const string expected2 =
				"<A'>: null a\r\n" +
				"<A>: null a\r\n" +
				"";

			const string expected3 =
				"<S'>: number openParen\r\n" +
				"<E>: number openParen\r\n" +
				"<F>: number openParen\r\n" +
				"<FOpp>: divide multiply\r\n" +
				"<S>: number openParen\r\n" +
				"<T>: number openParen\r\n" +
				"<TOpp>: add subtract\r\n" +
				"";

			Assert.That(Converter(_provider1), Is.EqualTo(expected1));
			Assert.That(Converter(_provider2), Is.EqualTo(expected2));
			Assert.That(Converter(_provider3), Is.EqualTo(expected3));
		}

		[Test]
		public void GetFollowSets()
		{
			string Converter(SegmentSetProvider provider)
			{
				var builder = new StringBuilder();

				foreach (var prod in _productions)
				{
					for (var i = 0; i < prod.Segments.Length; i++)
					{
						var set = provider.GetFollowSets(prod, i);

						if (set.Count > 0)
						{
							new ParseItem(prod, i).AppendTo(builder);
							builder.Append(": ");
							set.AppendTo(builder);
							builder.AppendLine();
						}
					}
				}

				return builder.ToString();
			}

			const string expected1 =
				"[<S'> -> \u2022 <S>]: null\r\n" +
				"[<I> -> \u2022 if <S>]: if other\r\n" +
				"[<I> -> if \u2022 <S>]: null\r\n" +
				"[<I> -> \u2022 if <S> else <S>]: if other\r\n" +
				"[<I> -> if \u2022 <S> else <S>]: else\r\n" +
				"[<I> -> if <S> \u2022 else <S>]: if other\r\n" +
				"[<I> -> if <S> else \u2022 <S>]: null\r\n" +
				"[<S> -> \u2022 other]: null\r\n" +
				"[<S> -> \u2022 <I>]: null\r\n" +
				"";

			const string expected2 =
				"[<A'> -> \u2022 <A>]: null\r\n" +
				"[<A> -> \u2022 <A> a]: a\r\n" +
				"[<A> -> <A> \u2022 a]: null\r\n" +
				"";

			const string expected3 =
				"[<S'> -> \u2022 <S>]: null\r\n" +
				"[<E> -> \u2022 <T>]: null\r\n" +
				"[<E> -> \u2022 <E> <TOpp> <T>]: add subtract\r\n" +
				"[<E> -> <E> \u2022 <TOpp> <T>]: number openParen\r\n" +
				"[<E> -> <E> <TOpp> \u2022 <T>]: null\r\n" +
				"[<F> -> \u2022 number]: null\r\n" +
				"[<F> -> \u2022 openParen <E> closeParen]: number openParen\r\n" +
				"[<F> -> openParen \u2022 <E> closeParen]: closeParen\r\n" +
				"[<F> -> openParen <E> \u2022 closeParen]: null\r\n" +
				"[<FOpp> -> \u2022 divide]: null\r\n" +
				"[<FOpp> -> \u2022 multiply]: null\r\n" +
				"[<S> -> \u2022 <E>]: null\r\n" +
				"[<T> -> \u2022 <F>]: null\r\n" +
				"[<T> -> \u2022 <T> <FOpp> <F>]: divide multiply\r\n" +
				"[<T> -> <T> \u2022 <FOpp> <F>]: number openParen\r\n" +
				"[<T> -> <T> <FOpp> \u2022 <F>]: null\r\n" +
				"[<TOpp> -> \u2022 add]: null\r\n" +
				"[<TOpp> -> \u2022 subtract]: null\r\n" +
				"";

			Assert.That(Converter(_provider1), Is.EqualTo(expected1));
			Assert.That(Converter(_provider2), Is.EqualTo(expected2));
			Assert.That(Converter(_provider3), Is.EqualTo(expected3));
		}

		[Test]
		public void InitialSegments()
		{
			string Converter(SegmentSetProvider provider)
			{
				var builder = new StringBuilder();
				provider.InitialSegments.AppendTo(builder);
				return builder.ToString();
			}

			const string expected1 = "<S'>";
			const string expected2 = "<A'>";
			const string expected3 = "<S'>";

			Assert.That(Converter(_provider1), Is.EqualTo(expected1));
			Assert.That(Converter(_provider2), Is.EqualTo(expected2));
			Assert.That(Converter(_provider3), Is.EqualTo(expected3));
		}

		[Test]
		public void CreateParseItems()
		{
			string Converter(SegmentSetProvider provider)
			{
				var builder = new StringBuilder();

				foreach (var nonTerminal in _nonTerminals)
				{
					var items = provider.CreateParseItems(nonTerminal);
					if (items.Length == 0) continue;

					nonTerminal.AppendTo(builder);
					builder.AppendLine(":");

					Array.Sort(items, (i1, i2) =>
					{
						int diff;

						diff = i1.Position - i2.Position;
						if (diff != 0) return diff;

						var p1 = i1.Production;
						var p2 = i2.Production;

						diff = p1.Segments.Length - p2.Segments.Length;
						if (diff != 0) return diff;

						diff = p1.Target.CompareTo(p2.Target);
						if (diff != 0) return diff;

						for (var i = 0; i < p1.Segments.Length; i++)
						{
							diff = p1.Segments[i].CompareTo(p2.Segments[i]);
							if (diff != 0) return diff;
						}

						return 0;
					});

					for (var i = 0; i < items.Length; i++)
					{
						items[i].AppendTo(builder);
						builder.AppendLine();
					}
				}

				return builder.ToString();
			}

			const string expected1 =
				"<S'>:\r\n" +
				"[<S'> -> \u2022 <S>]\r\n" +
				"<I>:\r\n" +
				"[<I> -> \u2022 if <S>]\r\n" +
				"[<I> -> \u2022 if <S> else <S>]\r\n" +
				"<S>:\r\n" +
				"[<S> -> \u2022 other]\r\n" +
				"[<S> -> \u2022 <I>]\r\n" +
				"";

			const string expected2 =
				"<A'>:\r\n" +
				"[<A'> -> \u2022 <A>]\r\n" +
				"<A>:\r\n" +
				"[<A> -> \u2022]\r\n" +
				"[<A> -> \u2022 <A> a]\r\n" +
				"";

			const string expected3 =
				"<S'>:\r\n" +
				"[<S'> -> \u2022 <S>]\r\n" +
				"<E>:\r\n" +
				"[<E> -> \u2022 <T>]\r\n" +
				"[<E> -> \u2022 <E> <TOpp> <T>]\r\n" +
				"<F>:\r\n" +
				"[<F> -> \u2022 number]\r\n" +
				"[<F> -> \u2022 openParen <E> closeParen]\r\n" +
				"<FOpp>:\r\n" +
				"[<FOpp> -> \u2022 divide]\r\n" +
				"[<FOpp> -> \u2022 multiply]\r\n" +
				"<S>:\r\n" +
				"[<S> -> \u2022 <E>]\r\n" +
				"<T>:\r\n" +
				"[<T> -> \u2022 <F>]\r\n" +
				"[<T> -> \u2022 <T> <FOpp> <F>]\r\n" +
				"<TOpp>:\r\n" +
				"[<TOpp> -> \u2022 add]\r\n" +
				"[<TOpp> -> \u2022 subtract]\r\n" +
				"";

			Assert.That(Converter(_provider1), Is.EqualTo(expected1));
			Assert.That(Converter(_provider2), Is.EqualTo(expected2));
			Assert.That(Converter(_provider3), Is.EqualTo(expected3));
		}

		#region Implementation

		[SetUp]
		public void Setup()
		{
			_terminalA = new Segment("a", true);
			_terminalAdd = new Segment("add", true);
			_terminalCloseParen = new Segment("closeParen", true);
			_terminalDivide = new Segment("divide", true);
			_terminalElseToken = new Segment("else", true);
			_terminalIfToken = new Segment("if", true);
			_terminalMultiply = new Segment("multiply", true);
			_terminalNumber = new Segment("number", true);
			_terminalOpenParen = new Segment("openParen", true);
			_terminalOtherToken = new Segment("other", true);
			_terminalSubtract = new Segment("subtract", true);

			_nonTerminalA = new Segment("A", false);
			_nonTerminalE = new Segment("E", false);
			_nonTerminalF = new Segment("F", false);
			_nonTerminalFOpp = new Segment("FOpp", false);
			_nonTerminalI = new Segment("I", false);
			_nonTerminalS = new Segment("S", false);
			_nonTerminalT = new Segment("T", false);
			_nonTerminalTOpp = new Segment("TOpp", false);

			_initialA = _nonTerminalA.GetInitial();
			_initialS = _nonTerminalS.GetInitial();

			var productions1 = CreateProductionSet1();
			var productions2 = CreateProductionSet2();
			var productions3 = CreateProductionSet3();

			_provider1 = new SegmentSetProvider(productions1);
			_provider2 = new SegmentSetProvider(productions2);
			_provider3 = new SegmentSetProvider(productions3);

			var prodList = new Dictionary<Production, bool>();

			foreach (var list in new[] { productions1, productions2, productions3 })
			{
				foreach (var prodArray in list.Values)
				{
					foreach (var prod in prodArray)
					{
						prodList[prod] = true;
					}
				}
			}

			_productions = new Production[prodList.Count];
			prodList.Keys.CopyTo(_productions, 0);
			Array.Sort(_productions, Compare);

			_nonTerminals = new[] { _initialA, _initialS, _nonTerminalA, _nonTerminalE, _nonTerminalF, _nonTerminalFOpp, _nonTerminalI, _nonTerminalS, _nonTerminalT, _nonTerminalTOpp };
		}

		static int Compare(Production p1, Production p2)
		{
			int diff;

			diff = p1.Target.CompareTo(p2.Target);
			if (diff != 0) return diff;

			diff = p1.Segments.Length - p2.Segments.Length;
			if (diff != 0) return diff;

			for (var i = 0; i < p1.Segments.Length; i++)
			{
				diff = p1.Segments[i].CompareTo(p2.Segments[i]);
				if (diff != 0) return diff;
			}

			return 0;
		}

		Dictionary<Segment, Production[]> CreateProductionSet1()
		{
			/*
			 * S' -> S
			 * S -> I
			 * S -> Other
			 * I -> If S
			 * I -> If S else S
			 */

			var result = new Dictionary<Segment, Production[]>();

			result.Add(_initialS, new Production[]
			{
				new Production(_initialS, ImmutableArray.Create(_nonTerminalS)),
			});

			result.Add(_nonTerminalS, new Production[]
			{
				new Production(_nonTerminalS, ImmutableArray.Create(_nonTerminalI)),
				new Production(_nonTerminalS, ImmutableArray.Create(_terminalOtherToken)),
			});

			result.Add(_nonTerminalI, new Production[]
			{
				new Production(_nonTerminalI, ImmutableArray.Create(_terminalIfToken, _nonTerminalS)),
				new Production(_nonTerminalI, ImmutableArray.Create(_terminalIfToken, _nonTerminalS, _terminalElseToken, _nonTerminalS)),
			});

			return result;
		}

		Dictionary<Segment, Production[]> CreateProductionSet2()
		{
			/*
			 * A' -> A
			 * A -> A a
			 * A ->
			 */

			var result = new Dictionary<Segment, Production[]>();

			result.Add(_initialA, new Production[]
			{
				new Production(_initialA, ImmutableArray.Create(_nonTerminalA)),
			});

			result.Add(_nonTerminalA, new Production[]
			{
				new Production(_nonTerminalA,  ImmutableArray.Create(_nonTerminalA, _terminalA)),
				new Production(_nonTerminalA, ImmutableArray<Segment>.Empty),
			});

			return result;
		}

		Dictionary<Segment, Production[]> CreateProductionSet3()
		{
			/*
			 * S' -> S
			 * S -> E
			 * E -> E TOpp T
			 * E -> T
			 * TOpp -> Add
			 * TOpp -> Subtract
			 * T -> T FOpp F
			 * T -> F
			 * FOpp -> Multiply
			 * FOpp -> Divide
			 * F -> Number
			 * F -> OpenParen E CloseParen
			 */
			var result = new Dictionary<Segment, Production[]>();

			result.Add(_initialS, new Production[]
			{
				new Production(_initialS, ImmutableArray.Create(_nonTerminalS)),
			});

			result.Add(_nonTerminalS, new Production[]
			{
				new Production(_nonTerminalS, ImmutableArray.Create(_nonTerminalE)),
			});

			result.Add(_nonTerminalE, new Production[]
			{
				new Production(_nonTerminalE, ImmutableArray.Create(_nonTerminalE, _nonTerminalTOpp, _nonTerminalT)),
				new Production(_nonTerminalE, ImmutableArray.Create(_nonTerminalT)),
			});

			result.Add(_nonTerminalTOpp, new Production[]
			{
				new Production(_nonTerminalTOpp, ImmutableArray.Create(_terminalAdd)),
				new Production(_nonTerminalTOpp, ImmutableArray.Create(_terminalSubtract)),
			});

			result.Add(_nonTerminalT, new Production[]
			{
				new Production(_nonTerminalT, ImmutableArray.Create(_nonTerminalT, _nonTerminalFOpp, _nonTerminalF)),
				new Production(_nonTerminalT, ImmutableArray.Create(_nonTerminalF)),
			});

			result.Add(_nonTerminalFOpp, new Production[]
			{
				new Production(_nonTerminalFOpp, ImmutableArray.Create(_terminalMultiply)),
				new Production(_nonTerminalFOpp, ImmutableArray.Create(_terminalDivide)),
			});

			result.Add(_nonTerminalF, new Production[]
			{
				new Production(_nonTerminalF, ImmutableArray.Create(_terminalNumber)),
				new Production(_nonTerminalF, ImmutableArray.Create(_terminalOpenParen, _nonTerminalE, _terminalCloseParen)),
			});

			return result;
		}

		Segment[] _nonTerminals;
		Production[] _productions;

		SegmentSetProvider _provider1;
		SegmentSetProvider _provider2;
		SegmentSetProvider _provider3;

		Segment _terminalA;
		Segment _terminalAdd;
		Segment _terminalCloseParen;
		Segment _terminalDivide;
		Segment _terminalElseToken;
		Segment _terminalIfToken;
		Segment _terminalMultiply;
		Segment _terminalNumber;
		Segment _terminalOpenParen;
		Segment _terminalOtherToken;
		Segment _terminalSubtract;

		Segment _nonTerminalA;
		Segment _nonTerminalE;
		Segment _nonTerminalF;
		Segment _nonTerminalFOpp;
		Segment _nonTerminalI;
		Segment _nonTerminalS;
		Segment _nonTerminalT;
		Segment _nonTerminalTOpp;

		Segment _initialA;
		Segment _initialS;

		#endregion
	}
}
