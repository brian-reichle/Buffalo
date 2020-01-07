// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class BoolConfigSettingTest
	{
		[Test]
		public void SetValidValue()
		{
			var setting = new BoolConfigSetting();

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.Label);
			token.Setup(x => x.Text).Returns("true");

			setting.Set(reporter.Object, token.Object);

			Assert.That(setting.Value, Is.EqualTo(true), "set to true");

			token.Setup(x => x.Type).Returns(SettingTokenType.Label);
			token.Setup(x => x.Text).Returns("false");

			setting.Set(reporter.Object, token.Object);

			Assert.That(setting.Value, Is.EqualTo(false), "set to false");
		}

		[Test]
		public void SetInvalidValue()
		{
			var setting = new BoolConfigSetting();

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.Label);
			token.Setup(x => x.Text).Returns("blat");
			token.Setup(x => x.FromPos).Returns(new CharPos(10, 11, 12));
			token.Setup(x => x.ToPos).Returns(new CharPos(13, 14, 15));

			reporter.Setup(x => x.AddError(11, 12, 14, 15, "'blat' is not a valid value.")).Verifiable();
			setting.Set(reporter.Object, token.Object);
			Assert.That(setting.Value, Is.EqualTo(false));
			reporter.Verify();
		}

		[Test]
		public void SetInvalidType()
		{
			var setting = new BoolConfigSetting();

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.String);
			token.Setup(x => x.Text).Returns("true");
			token.Setup(x => x.FromPos).Returns(new CharPos(10, 11, 12));
			token.Setup(x => x.ToPos).Returns(new CharPos(13, 14, 15));
			reporter.Setup(x => x.AddError(11, 12, 14, 15, "\"true\" is not a valid value.")).Verifiable();
			setting.Set(reporter.Object, token.Object);

			Assert.That(setting.Value, Is.EqualTo(false));
			reporter.Verify();
		}

		[Test]
		public void Reset()
		{
			var setting = new BoolConfigSetting();
			setting.DefaultValue = true;

			Assert.That(setting.Value, Is.EqualTo(false));

			setting.Reset();
			Assert.That(setting.Value, Is.EqualTo(true));
		}
	}
}
