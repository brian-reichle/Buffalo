// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Immutable;
using Buffalo.Core.Common.Test;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReConcatenationTest
	{
		[Test]
		public void Constructor()
		{
			var element1 = ReUtils.NewDummy('1');
			var element2 = ReUtils.NewDummy('2');

			var elements = ImmutableArray.Create(
				element1,
				element2);

			var choice = new ReConcatenation(elements);

			var enumerator = choice.Elements.GetEnumerator();
			Assert.That(enumerator.MoveNext(), Is.EqualTo(true), "should have element[0]");
			Assert.That(enumerator.Current, Is.SameAs(element1), "element[0] should be mock1");
			Assert.That(enumerator.MoveNext(), Is.EqualTo(true), "should have element[1]");
			Assert.That(enumerator.Current, Is.SameAs(element2), "element[1] should be mock2");
			Assert.That(enumerator.MoveNext(), Is.EqualTo(false), "should not have element[2]");
		}

		[Test]
		public void MatchesEmptyString()
		{
			var element1 = new Mock<ReElement>(MockBehavior.Strict);
			var element2 = new Mock<ReElement>(MockBehavior.Strict);

			var sequence = new ReConcatenation(ImmutableArray.Create(
				element1.Object,
				element2.Object));

			element1.Setup(x => x.MatchesEmptyString).Returns(true);
			element2.Setup(x => x.MatchesEmptyString).Returns(true);
			Assert.That(sequence.MatchesEmptyString, Is.EqualTo(true));

			element1.Setup(x => x.MatchesEmptyString).Returns(true);
			element2.Setup(x => x.MatchesEmptyString).Returns(false);
			Assert.That(sequence.MatchesEmptyString, Is.EqualTo(false));

			element1.Setup(x => x.MatchesEmptyString).Returns(false);
			Assert.That(sequence.MatchesEmptyString, Is.EqualTo(false));
		}

		[Test]
		public void GenerateNFA()
		{
			var element1 = ReUtils.NewDummy('1');
			var element2 = ReUtils.NewDummy('2');

			var sequence = new ReConcatenation(ImmutableArray.Create(
				element1,
				element2));

			const string expected =
				"0 -- [1] --> 1\r\n" +
				"1 -- [2] --> 2\r\n" +
				"";

			Assert.That(FARenderer.Render(ReUtils.Build(sequence)), Is.EqualTo(expected));
		}
	}
}
