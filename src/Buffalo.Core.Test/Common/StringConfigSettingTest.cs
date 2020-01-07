// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class StringConfigSettingTest
	{
		[Test]
		public void SetValidValue()
		{
			var setting = new StringConfigSetting();

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.String);
			token.Setup(x => x.Text).Returns("Text Value");

			setting.Set(reporter.Object, token.Object);

			Assert.That(setting.Value, Is.EqualTo("Text Value"));
		}

		[Test]
		public void SetLabelValue()
		{
			var setting = new StringConfigSetting();

			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var token = new Mock<IToken>(MockBehavior.Strict);

			token.Setup(x => x.Type).Returns(SettingTokenType.Label);
			token.Setup(x => x.FromPos).Returns(new CharPos(10, 11, 12));
			token.Setup(x => x.ToPos).Returns(new CharPos(13, 14, 15));
			token.Setup(x => x.Text).Returns("Value");

			reporter.Setup(x => x.AddError(11, 12, 14, 15, "'Value' is not a valid value.")).Verifiable();
			setting.Set(reporter.Object, token.Object);
			Assert.That(setting.Value, Is.EqualTo(null));
			reporter.Verify();
		}

		[Test]
		public void Reset()
		{
			var setting = new StringConfigSetting();
			setting.DefaultValue = "Text";

			Assert.That(setting.Value, Is.EqualTo(null));

			setting.Reset();
			Assert.That(setting.Value, Is.EqualTo("Text"));
		}
	}
}
