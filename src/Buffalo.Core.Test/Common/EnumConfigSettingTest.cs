// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public class EnumConfigSettingTest
	{
		[Test]
		public void SetValidValue()
		{
			var setting = new EnumConfigSetting<Blaticus>("description");

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.Label);
			token.Setup(x => x.Text).Returns("Gamma");

			setting.Set(reporter.Object, token.Object);

			Assert.That(setting.Value, Is.EqualTo(Blaticus.Gamma));
		}

		[Test]
		public void SetInvalidValue()
		{
			var setting = new EnumConfigSetting<Blaticus>("description");

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.Label);
			token.Setup(x => x.Text).Returns("Other");
			token.Setup(x => x.FromPos).Returns(new CharPos(10, 11, 12));
			token.Setup(x => x.ToPos).Returns(new CharPos(13, 14, 15));

			reporter.Setup(x => x.AddError(11, 12, 14, 15, "'Other' is not a valid description.")).Verifiable();

			setting.Set(reporter.Object, token.Object);

			Assert.That(setting.Value, Is.EqualTo(Blaticus.Alpha));
			reporter.Verify();
		}

		[Test]
		public void SetInvalidType()
		{
			var setting = new EnumConfigSetting<Blaticus>("description");

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.String);
			token.Setup(x => x.Text).Returns("Alpha");
			token.Setup(x => x.FromPos).Returns(new CharPos(10, 11, 12));
			token.Setup(x => x.ToPos).Returns(new CharPos(13, 14, 15));

			reporter.Setup(x => x.AddError(11, 12, 14, 15, "\"Alpha\" is not a valid description.")).Verifiable();
			setting.Set(reporter.Object, token.Object);
			Assert.That(setting.Value, Is.EqualTo(Blaticus.Alpha));
			reporter.Verify();
		}

		[Test]
		public void Reset()
		{
			var setting = new EnumConfigSetting<Blaticus>(string.Empty);
			setting.DefaultValue = Blaticus.Delta;

			Assert.That(setting.Value, Is.EqualTo(Blaticus.Alpha));

			setting.Reset();
			Assert.That(setting.Value, Is.EqualTo(Blaticus.Delta));
		}

		enum Blaticus
		{
			Alpha,
			Beta,
			Gamma,
			Delta,
		}
	}
}
