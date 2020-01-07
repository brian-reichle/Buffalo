// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common.Test;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReSingletonTest
	{
		[Test]
		public void Constructor()
		{
			var element = new ReSingleton(CharSet.New('1'));
			Assert.That(element.Label, Is.EqualTo(CharSet.New('1')));
		}

		[Test]
		public void MatchesEmptyString()
		{
			var singleton = new ReSingleton(CharSet.New('1'));
			Assert.That(singleton.MatchesEmptyString, Is.EqualTo(false));
		}

		[Test]
		public void GenerateNFA()
		{
			var element = new ReSingleton(CharSet.New('1'));

			const string expected =
				"0 -- [1] --> 1\r\n" +
				"";

			Assert.That(FARenderer.Render(ReUtils.Build(element)), Is.EqualTo(expected));
		}
	}
}
