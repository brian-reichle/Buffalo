// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Buffalo.Core.Common;
using Buffalo.Core.Common.Test;
using Buffalo.Core.Parser.Configuration;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class ParseGraphTest
	{
		[Test]
		public void ConstructDFA1()
		{
			var parseGraph = ParseGraph.ConstructGraph(ConstructSample1());

			const string expected =
				"0 (S)                     -- <A> --> 1                        \r\n" +
				"[<A'> -> • <A>, EOF]                 [<A'> -> <A> •, EOF]     \r\n" +
				"[<A> -> • a, EOF]                                             \r\n" +
				"[<A> -> • ( <A> ), EOF]                                       \r\n" +
				"0 (S)                     -- a --> 2                        \r\n" +
				"[<A'> -> • <A>, EOF]               [<A> -> a •, )/EOF]      \r\n" +
				"[<A> -> • a, EOF]                                           \r\n" +
				"[<A> -> • ( <A> ), EOF]                                     \r\n" +
				"0 (S)                     -- ( --> 3                        \r\n" +
				"[<A'> -> • <A>, EOF]               [<A> -> • a, )]          \r\n" +
				"[<A> -> • a, EOF]                  [<A> -> • ( <A> ), )]    \r\n" +
				"[<A> -> • ( <A> ), EOF]            [<A> -> ( • <A> ), )/EOF]\r\n" +
				"3                         -- a --> 2                        \r\n" +
				"[<A> -> • a, )]                    [<A> -> a •, )/EOF]      \r\n" +
				"[<A> -> • ( <A> ), )]                                       \r\n" +
				"[<A> -> ( • <A> ), )/EOF]                                   \r\n" +
				"3                         -- ( --> 3                        \r\n" +
				"[<A> -> • a, )]                    [<A> -> • a, )]          \r\n" +
				"[<A> -> • ( <A> ), )]              [<A> -> • ( <A> ), )]    \r\n" +
				"[<A> -> ( • <A> ), )/EOF]          [<A> -> ( • <A> ), )/EOF]\r\n" +
				"3                         -- <A> --> 4                        \r\n" +
				"[<A> -> • a, )]                      [<A> -> ( <A> • ), EOF]  \r\n" +
				"[<A> -> • ( <A> ), )]                                         \r\n" +
				"[<A> -> ( • <A> ), )/EOF]                                     \r\n" +
				"4                         -- ) --> 5                        \r\n" +
				"[<A> -> ( <A> • ), EOF]            [<A> -> ( <A> ) •, )/EOF]\r\n" +
				"";

			const string expectedStartPoints =
				"<A>: 0\r\n" +
				"";

			var renderer = new ParseRenderer();
			Assert.That(renderer.Render(parseGraph.Graph, 25), Is.EqualTo(expected));
			Assert.That(renderer.StartPointsRenderer(parseGraph), Is.EqualTo(expectedStartPoints));
		}

		[Test]
		public void ConstructDFA2()
		{
			var parseGraph = ParseGraph.ConstructGraph(ConstructSample2());

			const string expected =
				"0 (S)                      -- <E> --> 1                         \r\n" +
				"[<E'> -> • <E>, EOF]                  [<E'> -> <E> •, EOF]      \r\n" +
				"[<E> -> • n, EOF]                                               \r\n" +
				"[<E> -> • <V>, EOF]                                             \r\n" +
				"[<V> -> • id, EOF]                                              \r\n" +
				"0 (S)                      -- n --> 2                         \r\n" +
				"[<E'> -> • <E>, EOF]                [<E> -> n •, EOF]         \r\n" +
				"[<E> -> • n, EOF]                                             \r\n" +
				"[<E> -> • <V>, EOF]                                           \r\n" +
				"[<V> -> • id, EOF]                                            \r\n" +
				"0 (S)                      -- <V> --> 3                         \r\n" +
				"[<E'> -> • <E>, EOF]                  [<E> -> <V> •, EOF]       \r\n" +
				"[<E> -> • n, EOF]                                               \r\n" +
				"[<E> -> • <V>, EOF]                                             \r\n" +
				"[<V> -> • id, EOF]                                              \r\n" +
				"0 (S)                      -- id --> 4                         \r\n" +
				"[<E'> -> • <E>, EOF]                 [<V> -> id •, EOF]        \r\n" +
				"[<E> -> • n, EOF]                                              \r\n" +
				"[<E> -> • <V>, EOF]                                            \r\n" +
				"[<V> -> • id, EOF]                                             \r\n" +
				"5 (S)                      -- <S> --> 6                         \r\n" +
				"[<S'> -> • <S>, EOF]                  [<S'> -> <S> •, EOF]      \r\n" +
				"[<S> -> • id, EOF]                                              \r\n" +
				"[<S> -> • <V> := <E>, EOF]                                      \r\n" +
				"[<V> -> • id, :=]                                               \r\n" +
				"5 (S)                      -- id --> 7                         \r\n" +
				"[<S'> -> • <S>, EOF]                 [<S> -> id •, EOF]        \r\n" +
				"[<S> -> • id, EOF]                   [<V> -> id •, :=]         \r\n" +
				"[<S> -> • <V> := <E>, EOF]                                     \r\n" +
				"[<V> -> • id, :=]                                              \r\n" +
				"5 (S)                      -- <V> --> 8                         \r\n" +
				"[<S'> -> • <S>, EOF]                  [<S> -> <V> • := <E>, EOF]\r\n" +
				"[<S> -> • id, EOF]                                              \r\n" +
				"[<S> -> • <V> := <E>, EOF]                                      \r\n" +
				"[<V> -> • id, :=]                                               \r\n" +
				"8                          -- := --> 9                         \r\n" +
				"[<S> -> <V> • := <E>, EOF]           [<E> -> • n, EOF]         \r\n" +
				"                                     [<E> -> • <V>, EOF]       \r\n" +
				"                                     [<V> -> • id, EOF]        \r\n" +
				"                                     [<S> -> <V> := • <E>, EOF]\r\n" +
				"9                          -- n --> 2                         \r\n" +
				"[<E> -> • n, EOF]                   [<E> -> n •, EOF]         \r\n" +
				"[<E> -> • <V>, EOF]                                           \r\n" +
				"[<V> -> • id, EOF]                                            \r\n" +
				"[<S> -> <V> := • <E>, EOF]                                    \r\n" +
				"9                          -- <V> --> 3                         \r\n" +
				"[<E> -> • n, EOF]                     [<E> -> <V> •, EOF]       \r\n" +
				"[<E> -> • <V>, EOF]                                             \r\n" +
				"[<V> -> • id, EOF]                                              \r\n" +
				"[<S> -> <V> := • <E>, EOF]                                      \r\n" +
				"9                          -- id --> 4                         \r\n" +
				"[<E> -> • n, EOF]                    [<V> -> id •, EOF]        \r\n" +
				"[<E> -> • <V>, EOF]                                            \r\n" +
				"[<V> -> • id, EOF]                                             \r\n" +
				"[<S> -> <V> := • <E>, EOF]                                     \r\n" +
				"9                          -- <E> --> 10                        \r\n" +
				"[<E> -> • n, EOF]                     [<S> -> <V> := <E> •, EOF]\r\n" +
				"[<E> -> • <V>, EOF]                                             \r\n" +
				"[<V> -> • id, EOF]                                              \r\n" +
				"[<S> -> <V> := • <E>, EOF]                                      \r\n" +
				"";

			const string expectedStartPoints =
				"<E>: 0\r\n" +
				"<S>: 5\r\n" +
				"";

			var renderer = new ParseRenderer();
			Assert.That(renderer.Render(parseGraph.Graph, 26), Is.EqualTo(expected));
			Assert.That(renderer.StartPointsRenderer(parseGraph), Is.EqualTo(expectedStartPoints));
		}

		#region Implementation

		Config ConstructSample1()
		{
			/*
			 * A    := "(" A ")"
			 *      | "a"
			 */

			var nonTerminal = new Segment("A", false);
			var terminal = new Segment("a", true);
			var openParen = new Segment("(", true);
			var closeParen = new Segment(")", true);

			return new Config()
			{
				TopLevelSegments = SegmentSet.New(new Segment[] { nonTerminal }),
				Productions =
				{
					new ConfigProduction()
					{
						Target = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "A"),
						Segment = nonTerminal,
						Rules =
						{
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, "("),
										Segment = openParen,
									},
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "A"),
										Segment = nonTerminal,
									},
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, ")"),
										Segment = closeParen,
									},
								},
								Production = new Production(nonTerminal, ImmutableArray.Create(openParen, nonTerminal, closeParen)),
							},
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, "a"),
										Segment = terminal,
									},
								},
								Production = new Production(nonTerminal, ImmutableArray.Create(terminal)),
							},
						},
					},
				},
			};
		}

		Config ConstructSample2()
		{
			/*
			 * S    := "id"
			 *      | V ":=" E
			 *
			 * V    := "id"
			 *
			 * E    := V
			 *      | "n"
			 */

			var s = new Segment("S", false);
			var v = new Segment("V", false);
			var e = new Segment("E", false);

			var id = new Segment("id", true);
			var assign = new Segment(":=", true);
			var n = new Segment("n", true);

			return new Config()
			{
				TopLevelSegments = SegmentSet.New(new Segment[] { s, e, }),
				Productions =
				{
					new ConfigProduction()
					{
						Target = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "S"),
						Segment = s,
						Rules =
						{
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, "id"),
										Segment = id,
									},
								},
								Production = new Production(s, ImmutableArray.Create(id)),
							},
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "V"),
										Segment = v,
									},
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, ":="),
										Segment = assign,
									},
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "E"),
										Segment = e,
									},
								},
								Production = new Production(s, ImmutableArray.Create(v, assign, e)),
							},
						},
					},
					new ConfigProduction()
					{
						Target = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "V"),
						Segment = v,
						Rules =
						{
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, "id"),
										Segment = id,
									},
								},
								Production = new Production(v, ImmutableArray.Create(id)),
							},
						},
					},
					new ConfigProduction()
					{
						Target = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "E"),
						Segment = e,
						Rules =
						{
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.NonTerminal, null, null, "V"),
										Segment = v,
									},
								},
								Production = new Production(e, ImmutableArray.Create(v)),
							},
							new ConfigRule()
							{
								Segments =
								{
									new ConfigSegment()
									{
										Token = new ConfigToken(ConfigTokenType.Label, null, null, "n"),
										Segment = n,
									},
								},
								Production = new Production(e, ImmutableArray.Create(n)),
							},
						},
					},
				},
			};
		}

		sealed class ParseRenderer : FARenderer<ParseItemSet, Segment>
		{
			public string StartPointsRenderer(ParseGraph parseGraph)
			{
				var bulider = new StringBuilder();

				foreach (var pair in parseGraph)
				{
					pair.Key.AppendTo(bulider);
					bulider.Append(": ");
					bulider.Append(GetID(pair.Value).ToString(CultureInfo.InvariantCulture));
					bulider.AppendLine();
				}

				return bulider.ToString();
			}

			protected override string FormatState(Graph<ParseItemSet, Segment>.State state)
			{
				var builder = new StringBuilder();

				builder.Append(GetID(state));

				if (state.IsStartState)
				{
					builder.Append(" (S)");
				}

				builder.AppendLine();
				state.Label.AppendTo(builder);
				return builder.ToString();
			}
		}

		#endregion
	}
}
