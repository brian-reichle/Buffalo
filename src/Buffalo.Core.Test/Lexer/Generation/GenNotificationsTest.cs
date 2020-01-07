// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class GenNotificationsTest
	{
		[Test]
		public void ReErrors()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddError(6, 2, 6, 4, "unexpected char '{' at position 0")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.ReErrors(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void MatchEmptyString()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddWarning(6, 2, 6, 5, "This regular expression claims to match the empty string, this is not supported and usually indicates a typeo in the regular expression")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.MatchEmptyString(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void UnknownSetting()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddError(3, 1, 3, 8, "'Disarray' is not a recognised option.")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.UnknownSetting(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void DuplicatedSetting()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddWarning(3, 1, 3, 4, "Name has already been defined.")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.DuplicatedSetting(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void UnknownVisibility()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddError(3, 14, 3, 16, "'BOB' is not a valid visibility.")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.UnknownVisibility(),
				reporter.Object,
				environment.Object);
		}

		[Test]
		public void NoStates()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddError(1, 1, 1, 1, "No states defined.")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.NoStates(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void NoRules()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns((string)null);
			reporter.Setup(x => x.AddError(4, 7, 4, 13, "The state 'INITIAL' does not define any rules.")).Verifiable();

			GeneratorRunner.Run<LexerGenerator>(
				LexerTestFiles.NoRules(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}
	}
}
