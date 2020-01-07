// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common.Test;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReEmptyStringTest
	{
		[Test]
		public void MatchesEmptyString()
		{
			var emptyLanguage = ReEmptyString.Instance;
			Assert.That(emptyLanguage.MatchesEmptyString, Is.EqualTo(true));
		}

		[Test]
		public void GenerateNFA()
		{
			const string expected =
				"0 -- e --> 1\r\n";

			Assert.That(FARenderer.Render(ReUtils.Build(ReEmptyString.Instance)), Is.EqualTo(expected));
		}
	}
}
