// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class LexerGeneratorTest
	{
		[Test]
		public void GenerateLexerConfig()
		{
			var errorReporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns("Buffalo.Core.Lexer.Configuration.AutoConfigScanner.{0}.table");

			GeneratorRunner.Run<LexerGenerator>(LexerTestFiles.Scanner(), errorReporter.Object, environment.Object);
		}

		[Test]
		public void GenerateParserConfig()
		{
			var errorReporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns("Buffalo.Core.Parser.Configuration.AutoConfigScanner.{0}.table");

			GeneratorRunner.Run<LexerGenerator>(LexerTestFiles.Parser(), errorReporter.Object, environment.Object);
		}
	}
}
