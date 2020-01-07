// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using System.Text;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;

namespace Buffalo.Core.Lexer.Test
{
	static class CommonLexerTest
	{
		public static void AssertGeneration(ResourceSet set, bool embedTable)
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			string expectedResourceName;

			if (embedTable)
			{
				var builder = new StringBuilder("Buffalo.Core.Test.Lexer.Generation.");
				builder.Append(set.Code.ResourceName, LexerTestFiles.Namespace.Length, set.Code.ResourceName.Length - LexerTestFiles.Namespace.Length - 5);
				builder.Append(".{0}.table");

				expectedResourceName = builder.ToString();
			}
			else
			{
				expectedResourceName = null;
			}

			environment.Setup(x => x.GetResourceName(".{0}.table")).Returns(expectedResourceName);

			GeneratorRunner.Run<LexerGenerator>(set, reporter.Object, environment.Object);
		}
	}
}
