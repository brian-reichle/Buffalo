// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common.Test;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReKleeneStarTest
	{
		[Test]
		public void Constructor()
		{
			var element = new Mock<ReElement>();
			var kleeneStar = new ReKleeneStar(element.Object);
			Assert.That(kleeneStar.Element, Is.SameAs(element.Object));
		}

		[Test]
		public void MatchesEmptyString()
		{
			var element = new Mock<ReElement>();
			var kleenStar = new ReKleeneStar(element.Object);
			Assert.That(kleenStar.MatchesEmptyString, Is.EqualTo(true));
		}

		[Test]
		public void GenerateNFA()
		{
			var element = ReUtils.NewDummy('1');
			var kleeneStar = new ReKleeneStar(element);

			const string expected =
				"0 -- e --> 1\r\n" +
				"1 -- [1] --> 1\r\n" +
				"1 -- e --> 2\r\n" +
				"";

			Assert.That(FARenderer.Render(ReUtils.Build(kleeneStar)), Is.EqualTo(expected));
		}
	}
}
