// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class ParserGeneratorTest
	{
		[Test]
		public void GenerateLexerConfig()
		{
			var errorReporter = new Mock<IErrorReporter>();
			var environment = new Mock<ICodeGeneratorEnv>();

			environment.Setup(x => x.GetResourceName(".table")).Returns("Buffalo.Core.Lexer.Configuration.AutoConfigParser.table");

			GeneratorRunner.Run<ParserGenerator>(ParserTestFiles.Scanner(), errorReporter.Object, environment.Object);
		}

		[Test]
		public void GenerateParserConfig()
		{
			var errorReporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".table")).Returns("Buffalo.Core.Parser.Configuration.AutoConfigParser.table");

			GeneratorRunner.Run<ParserGenerator>(ParserTestFiles.Parser(), errorReporter.Object, environment.Object);
		}
	}
}
