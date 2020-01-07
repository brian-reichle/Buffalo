// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common.Test;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReEmptyLanguageTest
	{
		[Test]
		public void MatchesEmptyString()
		{
			var emptyLanguage = ReEmptyLanguage.Instance;
			Assert.That(emptyLanguage.MatchesEmptyString, Is.EqualTo(false));
		}

		[Test]
		public void GenerateNFA()
		{
			const string expected =
				"";

			Assert.That(FARenderer.Render(ReUtils.Build(ReEmptyLanguage.Instance)), Is.EqualTo(expected));
		}
	}
}
