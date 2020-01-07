// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture]
	public sealed class GramourParserTest
	{
		[TestCase("ParserErrorWithoutEOF")]
		[TestCase("ParserGotoOnlyStates")]
		[TestCase("ParserIgnoredSegments")]
		[TestCase("ParserMultipleEntry")]
		[TestCase("ParserOptionalSegments")]
		[TestCase("ParserReductionDuringError")]
		[TestCase("ParserShiftReduceConflict")]
		[TestCase("ParserMerge")]
		[TestCase("ParserNoMerge")]
		[TestCase("ParserMergeNonTerminals")]
		public void Generate(string setName)
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			var set = ParserTestFiles.GetNamedResourceSet(setName);

			var builder = new StringBuilder("Buffalo.Core.Test.Parser.Generation.");
			builder.Append(set.Code.ResourceName, ParserTestFiles.Namespace.Length, set.Code.ResourceName.Length - ParserTestFiles.Namespace.Length - 5);
			builder.Append(".table");

			environment.Setup(x => x.GetResourceName(".table")).Returns(builder.ToString());

			GeneratorRunner.Run<ParserGenerator>(set, reporter.Object, environment.Object);
		}

		[Test]
		public void ErrorWithoutEOF()
		{
			const string Source = @"
OpenParen: (
Label: a
Comma: ,
Label: b
Label: b
Comma: ,
Label: c
CloseParen: )
";

			var parser = ParserFactory.NewParser(typeof(Generated.ErrorWithoutEOF.AutoParser), ErrorWithoutEOFReduction);
			var value = Parse(parser, null, Source);
			Assert.That(value, Is.EquivalentTo(new string[] { "<error>", "\"c\"" }));
		}

		[Test]
		public void GotoOnlyStates()
		{
			const string Source = @"
A: a
A: a
B: b
";

			var parser = ParserFactory.NewParser(typeof(Generated.GotoOnlyStates.AutoParser), GotoOnlyStatesReduction);

			var value = Parse(parser, null, Source);
			Assert.That(value, Is.EqualTo("12"));
		}

		[Test]
		public void IgnoredSegments()
		{
			const string Source1 = @"
A: a
A: 1
";

			const string Source2 = @"
B: b
B: 2
";

			var parser = ParserFactory.NewParser(typeof(Generated.IgnoredSegments.AutoParser), OptionalSegments);

			Assert.That(Parse(parser, null, Source1), Is.EqualTo("a"));
			Assert.That(Parse(parser, null, Source2), Is.EqualTo("b"));
		}

		[TestCaseSource(nameof(MergeNoMergeSource))]
		public void Merge(string token, string text)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Open: (");
			builder.Append(token);
			builder.Append(": ");
			builder.AppendLine(text);
			builder.AppendLine("Close: )");

			var parser = ParserFactory.NewParser(typeof(Generated.Merge.AutoParser), MergeReduction);
			var value = (Token)parser.Parse(null, Token.GetTokens(builder.ToString()));
			Assert.That(value, Is.Not.Null);
			Assert.That(value.TokenTypeName, Is.EqualTo(token));
			Assert.That(value.Value, Is.EqualTo(text));
		}

		[TestCaseSource(nameof(MergeNoMergeSource))]
		public void NoMerge(string token, string text)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Open: (");
			builder.Append(token);
			builder.Append(": ");
			builder.AppendLine(text);
			builder.AppendLine("Close: )");

			var parser = ParserFactory.NewParser(typeof(Generated.NoMerge.AutoParser), MergeReduction);
			var value = (Token)parser.Parse(null, Token.GetTokens(builder.ToString()));
			Assert.That(value, Is.Not.Null);
			Assert.That(value.TokenTypeName, Is.EqualTo(token));
			Assert.That(value.Value, Is.EqualTo(text));
		}

		[TestCaseSource(nameof(MergeNoMergeSource))]
		public void Merge_NoParen(string token, string text)
		{
			var builder = new StringBuilder();
			builder.Append(token);
			builder.Append(": ");
			builder.AppendLine(text);

			var parser = ParserFactory.NewParser(typeof(Generated.Merge.AutoParser), MergeReduction);
			var value = (Token)parser.Parse(null, Token.GetTokens(builder.ToString()));
			Assert.That(value, Is.Not.Null);
			Assert.That(value.TokenTypeName, Is.EqualTo(token));
			Assert.That(value.Value, Is.EqualTo(text));
		}

		[TestCaseSource(nameof(MergeNoMergeSource))]
		public void NoMerge_NoParen(string token, string text)
		{
			var builder = new StringBuilder();
			builder.Append(token);
			builder.Append(": ");
			builder.AppendLine(text);

			var parser = ParserFactory.NewParser(typeof(Generated.NoMerge.AutoParser), MergeReduction);
			var value = (Token)parser.Parse(null, Token.GetTokens(builder.ToString()));
			Assert.That(value, Is.Not.Null);
			Assert.That(value.TokenTypeName, Is.EqualTo(token));
			Assert.That(value.Value, Is.EqualTo(text));
		}

		[Test]
		public void MergeNonTerminals()
		{
			const string Source = @"
Pre: [
X1: a
Y1: b
Post: ]
";

			var parser = ParserFactory.NewParser(typeof(Generated.MergeNonTerminals.AutoParser), MergeNonTerminalReduction);
			var value = (string)parser.Parse(null, Token.GetTokens(Source));
			Assert.That(value, Is.EqualTo("a-b"));
		}

		[Test]
		public void MultipleEntry_Expression()
		{
			const string Source = @"
Number: 5
Multiply: *
Number: 9
Subtract: -
Number: 3
";

			var parser = ParserFactory.NewParser(typeof(Generated.MultipleEntry.AutoParser), MultipleEntryReduction);
			Assert.That(Parse(parser, "Expression", Source), Is.EqualTo(42));
		}

		[Test]
		public void MultipleEntry_Expression_Ex()
		{
			const string Source = @"
Number: 5
Multiply: *
Number: 9
Subtract: -
Number: 3
";

			var parser = ParserFactory.NewParser(typeof(Generated.MultipleEntry.AutoParser), MultipleEntryReduction);
			Assert.That(() => Parse(parser, "Equation", Source), Throws.InvalidOperationException.With.Message.EqualTo("unexpected token: EOF"));
		}

		[Test]
		public void MultipleEntry_Equation()
		{
			const string Source = @"
Number: 45
EqualTo: ==
Number: 3
";

			var parser = ParserFactory.NewParser(typeof(Generated.MultipleEntry.AutoParser), MultipleEntryReduction);
			Assert.That(Parse(parser, "Equation", Source), Is.False);
		}

		[Test]
		public void MultipleEntry_Equation_Ex()
		{
			const string Source = @"
Number: 45
EqualTo: ==
Number: 3
";

			var parser = ParserFactory.NewParser(typeof(Generated.MultipleEntry.AutoParser), MultipleEntryReduction);
			Assert.That(() => Parse(parser, "Expression", Source), Throws.InvalidOperationException.With.Message.EqualTo("unexpected token: EqualTo"));
		}

		public void OptionalSegments()
		{
			const string Source1 = @"
A: a
A: 1
";

			const string Source2 = @"
A: a
";

			const string Source3 = @"
B: b
B: 3
";

			const string Source4 = @"
B: b
";

			var parser = ParserFactory.NewParser(typeof(Generated.OptionalSegments.AutoParser), OptionalSegments);

			Assert.That(Parse(parser, null, Source1), Is.EqualTo("1"));
			Assert.That(Parse(parser, null, Source2), Is.EqualTo("null"));
			Assert.That(Parse(parser, null, Source3), Is.EqualTo("3"));
			Assert.That(Parse(parser, null, Source4), Is.EqualTo("null"));
		}

		[Test]
		public void ReductionDuringError()
		{
			const string Source = @"
y: y
Error: err
";

			var parser = ParserFactory.NewParser(typeof(Generated.ReductionDuringError.AutoParser), ReductionDuringErrorReduction);

			var value = Parse(parser, null, Source);
			Assert.That(value, Is.EqualTo("XA[Y:y]/<X:err>BY/XCY"));
		}

		[Test]
		public void ShiftReduceConflict()
		{
			const string Source = @"
IfKeyword: if
IfKeyword: if
Other: A
ElseKeyword: else
Other: B
";

			var parser = ParserFactory.NewParser(typeof(Generated.ShiftReduceConflict.AutoParser), ShiftReduceConflictReduction);

			var value = Parse(parser, null, Source);
			Assert.That(value, Is.EqualTo("((-))"));
		}

		#region Implementation

		static IEnumerable<TestCaseData> MergeNoMergeSource()
		{
			yield return new TestCaseData("Alpha", "a");
			yield return new TestCaseData("Beta", "b");
			yield return new TestCaseData("Gamma", "g");
			yield return new TestCaseData("Delta", "d");
		}

		static object Parse(IParser parser, string entry, string source)
		{
			var tokens = Token.GetTokens(source);

			try
			{
				return parser.Parse(entry, tokens);
			}
			catch
			{
				if (parser.SupportsTrace)
				{
					Trace.WriteLine(parser.Trace);
				}

				throw;
			}
		}

		object ErrorWithoutEOFReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Values_1": return args[1];
				case "Reduce_List_1": ((IList<string>)args[0]).Add((string)args[2]); return args[0];
				case "Reduce_List_2": return new List<string>() { (string)args[0] };
				case "Reduce_Element_1": return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", ((Token)args[0]).Value);
				case "Reduce_Element_2": return "<error>";
				default: throw new InvalidOperationException();
			}
		}

		object GotoOnlyStatesReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_L_1": return string.Format(CultureInfo.InvariantCulture, "{0}1", args);
				case "Reduce_L_2": return string.Format(CultureInfo.InvariantCulture, "{0}2", args);
				case "Reduce_L_3": return string.Empty;
				default: throw new InvalidOperationException();
			}
		}

		object MergeReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Document_2":
				case "Reduce_Alt_1":
					return args[0];

				default: throw new InvalidOperationException();
			}
		}

		object MergeNonTerminalReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Document_1": return args[1];

				case "Reduce_B1_1":
				case "Reduce_B2_1":
					return ((Token)args[0]).Value + "-" + ((Token)args[1]).Value;

				default: throw new InvalidOperationException();
			}
		}

		object MultipleEntryReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Equation_1":
				case "Reduce_Expression_1":
				case "Reduce_Term_1":
					var lhs = (int)args[0];
					var rhs = (int)args[2];

					switch (((Token)args[1]).TokenTypeName)
					{
						case "Add": return lhs + rhs;
						case "Subtract": return lhs - rhs;
						case "Multiply": return lhs * rhs;
						case "Divide": return lhs / rhs;
						case "EqualTo": return lhs == rhs;
						default: throw new InvalidOperationException();
					}

				case "Reduce_Factor_1": return int.Parse(((Token)args[0]).Value, CultureInfo.InvariantCulture);

				default: throw new InvalidOperationException();
			}
		}

		object OptionalSegments(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Document_1":
				case "Reduce_Document_2":
					var t = (Token)args[0];
					return t?.Value;
				default: throw new InvalidOperationException();
			}
		}

		object ReductionDuringErrorReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Test_1": return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", args);
				case "Reduce_A_1": return string.Format(CultureInfo.InvariantCulture, "{0}A{1}", args);
				case "Reduce_B_1": return string.Format(CultureInfo.InvariantCulture, "{0}B{1}", args);
				case "Reduce_C_1": return string.Format(CultureInfo.InvariantCulture, "{0}C{1}", args);
				case "Reduce_X_1": return string.Format(CultureInfo.InvariantCulture, "[X:{0}]", ((Token)args[0]).Value);
				case "Reduce_X_2": return string.Format(CultureInfo.InvariantCulture, "<X:{0}>", ((Token)args[0]).Value);
				case "Reduce_X_3": return "X";
				case "Reduce_Y_1": return string.Format(CultureInfo.InvariantCulture, "[Y:{0}]", ((Token)args[0]).Value);
				case "Reduce_Y_2": return "Y";
				default: throw new InvalidOperationException();
			}
		}

		object ShiftReduceConflictReduction(string s, object[] args)
		{
			switch (s)
			{
				case "Reduce_Statement_1": return args[0];
				case "Reduce_If_1": return string.Format(CultureInfo.InvariantCulture, "({1})", args);
				case "Reduce_If_2": return string.Format(CultureInfo.InvariantCulture, "({1}-{3})", args);
				default: throw new InvalidOperationException();
			}
		}

		#endregion
	}
}
