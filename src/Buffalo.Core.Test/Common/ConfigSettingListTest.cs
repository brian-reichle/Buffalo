// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public sealed class ConfigSettingListTest
	{
		[Test]
		public void SetValidOption()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var setting1 = new Mock<IConfigSetting>(MockBehavior.Strict);
			var setting2 = new Mock<IConfigSetting>(MockBehavior.Strict);

			var list = new ListSubclass();
			list.AddSetting("setting1", setting1.Object);
			list.AddSetting("setting2", setting2.Object);

			labelToken.Setup(x => x.Text).Returns("setting1");
			setting1.Setup(x => x.Set(reporter.Object, valueToken.Object)).Returns(true).Verifiable();

			list.Set(reporter.Object, labelToken.Object, valueToken.Object);

			setting1.Verify();
		}

		[Test]
		public void InvalidOption()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);

			var list = new ListSubclass();

			labelToken.Setup(x => x.Text).Returns("setting");
			labelToken.Setup(x => x.FromPos).Returns(new CharPos(1, 2, 3));
			labelToken.Setup(x => x.ToPos).Returns(new CharPos(4, 5, 6));
			reporter.Setup(x => x.AddError(2, 3, 5, 6, "'setting' is not a recognised option.")).Verifiable();

			list.Set(reporter.Object, labelToken.Object, valueToken.Object);

			reporter.Verify();
		}

		[Test]
		public void DuplicateOption()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var setting = new Mock<IConfigSetting>(MockBehavior.Strict);

			var list = new ListSubclass();
			list.AddSetting("setting", setting.Object);

			labelToken.Setup(x => x.Text).Returns("setting");
			setting.Setup(x => x.Set(reporter.Object, valueToken.Object)).Returns(true).Verifiable();

			list.Set(reporter.Object, labelToken.Object, valueToken.Object);
			setting.Verify();
			setting.Reset();

			labelToken.Setup(x => x.Text).Returns("setting");
			labelToken.Setup(x => x.FromPos).Returns(new CharPos(1, 2, 3));
			labelToken.Setup(x => x.ToPos).Returns(new CharPos(4, 5, 6));
			setting.Setup(x => x.Set(reporter.Object, valueToken.Object)).Returns(true).Verifiable();
			reporter.Setup(x => x.AddWarning(2, 3, 5, 6, "setting has already been defined.")).Verifiable();

			list.Set(reporter.Object, labelToken.Object, valueToken.Object);

			setting.Verify();
			reporter.Verify();
		}

		class ListSubclass : ConfigSettingList
		{
			public new void AddSetting(string name, IConfigSetting setting)
			{
				base.AddSetting(name, setting);
			}
		}
	}
}
