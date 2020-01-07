// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Configuration.Test
{
	[TestFixture]
	public class ConfigManagerTest
	{
		[Test]
		public void ClassName()
		{
			var manager = new ConfigManager();
			manager.Reset();

			Assert.That(manager.ClassName, Is.EqualTo("Scanner"));

			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			labelToken.Setup(x => x.Text).Returns("Name");
			valueToken.Setup(x => x.Text).Returns("NewName");
			valueToken.Setup(x => x.Type).Returns(SettingTokenType.String);

			manager.Set(reporter.Object, labelToken.Object, valueToken.Object);

			Assert.That(manager.ClassName, Is.EqualTo("NewName"));
		}

		[Test]
		public void ClassNamespace()
		{
			var manager = new ConfigManager();
			manager.Reset();

			Assert.That(manager.ClassNamespace, Is.EqualTo("Unspecified"));

			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			labelToken.Setup(x => x.Text).Returns("Namespace");
			valueToken.Setup(x => x.Text).Returns("NewNamespace");
			valueToken.Setup(x => x.Type).Returns(SettingTokenType.String);

			manager.Set(reporter.Object, labelToken.Object, valueToken.Object);

			Assert.That(manager.ClassNamespace, Is.EqualTo("NewNamespace"));
		}

		[Test]
		public void Visibility()
		{
			var manager = new ConfigManager();
			manager.Reset();

			Assert.That(manager.Visibility, Is.EqualTo(ClassVisibility.Internal));

			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			labelToken.Setup(x => x.Text).Returns("Visibility");
			valueToken.Setup(x => x.Text).Returns("Public");
			valueToken.Setup(x => x.Type).Returns(SettingTokenType.Label);

			manager.Set(reporter.Object, labelToken.Object, valueToken.Object);

			Assert.That(manager.Visibility, Is.EqualTo(ClassVisibility.Public));
		}

		[Test]
		public void ElementSize()
		{
			var manager = new ConfigManager();
			manager.Reset();

			Assert.That(manager.ElementSize, Is.EqualTo(TableElementSize.Short));

			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			labelToken.Setup(x => x.Text).Returns("ElementSize");
			valueToken.Setup(x => x.Text).Returns("Byte");
			valueToken.Setup(x => x.Type).Returns(SettingTokenType.Label);

			manager.Set(reporter.Object, labelToken.Object, valueToken.Object);

			Assert.That(manager.ElementSize, Is.EqualTo(TableElementSize.Byte));
		}

		[Test]
		public void TableCompression()
		{
			var manager = new ConfigManager();
			manager.Reset();

			Assert.That(manager.TableCompression, Is.EqualTo(Compression.Auto));

			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			labelToken.Setup(x => x.Text).Returns("TableCompression");
			valueToken.Setup(x => x.Text).Returns("None");
			valueToken.Setup(x => x.Type).Returns(SettingTokenType.Label);

			manager.Set(reporter.Object, labelToken.Object, valueToken.Object);

			Assert.That(manager.TableCompression, Is.EqualTo(Compression.None));
		}

		[Test]
		public void CacheTables()
		{
			var manager = new ConfigManager();
			manager.Reset();

			Assert.That(manager.CacheTables, Is.EqualTo(false));

			var labelToken = new Mock<IToken>(MockBehavior.Strict);
			var valueToken = new Mock<IToken>(MockBehavior.Strict);
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			labelToken.Setup(x => x.Text).Returns("CacheTables");
			valueToken.Setup(x => x.Text).Returns("true");
			valueToken.Setup(x => x.Type).Returns(SettingTokenType.Label);

			manager.Set(reporter.Object, labelToken.Object, valueToken.Object);

			Assert.That(manager.CacheTables, Is.EqualTo(true));
		}
	}
}
