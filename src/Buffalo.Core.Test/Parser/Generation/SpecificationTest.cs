// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public class SpecificationTest
	{
		[Test]
		public void DuplicateEntryPoints()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddWarning(2, 7, 2, 9, "The non-terminal <A> is already an entry point.")).Verifiable();
			environment.Setup(x => x.GetResourceName(".table")).Returns((string)null);

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.DuplicateEntryPoints(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void DuplicateOption()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddWarning(2, 1, 2, 4, "Name has already been defined.")).Verifiable();
			environment.Setup(x => x.GetResourceName(".table")).Returns((string)null);

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.DuplicateOption(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void DuplicateProduction()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".table")).Returns((string)null);
			reporter.Setup(x => x.AddWarning(6, 1, 6, 3, "The non-terminal <A> has already been defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.DuplicateProduction(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void DuplicateReduction()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".table")).Returns((string)null);
			reporter.Setup(x => x.AddWarning(5, 1, 5, 3, "The production '<A> ->' has already been defined.")).Verifiable();
			reporter.Setup(x => x.AddWarning(7, 4, 7, 4, "The production '<A> -> a' has already been defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.DuplicateReduction(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void InvalidProductionArg()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(6, 15, 6, 16, "Invalid argument reference")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.InvalidProductionArg(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void NoProductions()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(1, 1, 1, 1, "No productions defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.NoProductions(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void PseudoReachable()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(1, 1, 1, 1, "All accept actions were optomised away.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.ParserPseudoReachable(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void ReDefinedDifferentType()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(8, 16, 8, 16, "This type conflicts with a previous definition of <NonTerminal>.")).Verifiable();
			reporter.Setup(x => x.AddWarning(8, 1, 8, 13, "The non-terminal <NonTerminal> has already been defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.ReDefinedDifferentType(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void ReDefinedSameType()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".table")).Returns((string)null);
			reporter.Setup(x => x.AddWarning(8, 1, 8, 13, "The non-terminal <NonTerminal> has already been defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.ReDefinedSameType(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void ReduceAcceptConflict()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(6, 6, 6, 8, "<A> -> <A> causes a reduce-accept conflict.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.ReduceAcceptConflict(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void ReduceReduceConflict()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(10, 1, 10, 3, "<B> -> causes a reduce-reduce conflict with <A> -> a.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.ReduceReduceConflict(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void UndefinedEntry()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(2, 7, 2, 9, "The non-terminal <B> is not defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.UndefinedEntry(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void UndefinedProduction()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(1, 9, 1, 11, "The non-terminal <B> is not defined.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.UndefinedProduction(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void UnknownOption()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			reporter.Setup(x => x.AddError(1, 1, 1, 8, "'Disarray' is not a recognised option.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.UnknownOption(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}

		[Test]
		public void UnreachableNonTerminal()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			environment.Setup(x => x.GetResourceName(".table")).Returns((string)null);
			reporter.Setup(x => x.AddWarning(6, 1, 6, 3, "The non-terminal <B> is not reachable.")).Verifiable();

			GeneratorRunner.Run<ParserGenerator>(
				ParserTestFiles.UnreachableNonTerminal(),
				reporter.Object,
				environment.Object);

			reporter.Verify();
		}
	}
}
