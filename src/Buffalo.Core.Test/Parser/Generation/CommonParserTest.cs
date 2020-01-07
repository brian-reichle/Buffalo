// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;
using System.Text;
using Buffalo.Core.Test;
using Buffalo.TestResources;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Parser.Test
{
	[TestFixture(typeof(Generated.Base.AutoParser), "ParserBase")]
	[TestFixture(typeof(Generated.Byte.AutoParser), "ParserByte")]
	[TestFixture(typeof(Generated.Int.AutoParser), "ParserInt")]
	[TestFixture(typeof(Generated.Short.AutoParser), "ParserShort")]
	[TestFixture(typeof(Generated.CacheTables.AutoParser), "ParserCacheTables")]
	[TestFixture(typeof(Generated.ShowTable.AutoParser), "ParserShowTable")]
	[TestFixture(typeof(Generated.SuppressEmbedding.AutoParser), "ParserSuppressEmbedding", false, false)]
	[TestFixture(typeof(Generated.SVG.AutoParser), "ParserSVG")]
	[TestFixture(typeof(Generated.Trace.AutoParser), "ParserTrace", true, true)]
	[TestFixture(typeof(Generated.CTB.AutoParser), "ParserCTB")]
	[TestFixture(typeof(Generated.None.AutoParser), "ParserNone")]
	[TestFixture(typeof(Generated.Simple.AutoParser), "ParserSimple")]
	[TestFixture(typeof(Generated.Cast.AutoParser), "ParserCast")]
	[TestFixture(typeof(Generated.Field.AutoParser), "ParserField")]
	[TestFixture(typeof(Generated.Internal.AutoParser), "ParserInternal")]
	[TestFixture(typeof(Generated.Public.AutoParser), "ParserPublic")]
	public sealed class CommonParserTest<T>
	{
		public CommonParserTest(string setName)
			: this(setName, true, false)
		{
		}

		public CommonParserTest(string setName, bool embedTable, bool trace)
		{
			_setName = setName;
			_embedTable = embedTable;
			_trace = trace;
		}

		[Test]
		public void Generate()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);
			var environment = new Mock<ICodeGeneratorEnv>(MockBehavior.Strict);

			var set = ParserTestFiles.GetNamedResourceSet(_setName);

			if (_embedTable)
			{
				var builder = new StringBuilder("Buffalo.Core.Test.Parser.Generation.");
				builder.Append(set.Code.ResourceName, ParserTestFiles.Namespace.Length, set.Code.ResourceName.Length - ParserTestFiles.Namespace.Length - 5);
				builder.Append(".table");

				environment.Setup(x => x.GetResourceName(".table")).Returns(builder.ToString());
			}

			GeneratorRunner.Run<ParserGenerator>(set, reporter.Object, environment.Object);
		}

		[Test]
		public void Parse()
		{
			const string Source = @"
OpenParen:
Number: 6
Add:
Number: 7
CloseParen:
Multiply:
Number: 5
Power:
Number: 2
GreaterThan:
Number: 0
";

			const string Expected = @"
-Reduce_Equation_1
 +Reduce_Expression_2
 |\Reduce_Term_1
 | +Reduce_Term_2
 | |\Reduce_Factor_2
 | | \Reduce_Value_2
 | |  +OpenParen: 
 | |  +Reduce_Expression_1
 | |  |+Reduce_Expression_2
 | |  ||\Reduce_Term_2
 | |  || \Reduce_Factor_2
 | |  ||  \Reduce_Value_1
 | |  ||   \Number: 6
 | |  |+Reduce_TermOp_1
 | |  ||\Add: 
 | |  |\Reduce_Term_2
 | |  | \Reduce_Factor_2
 | |  |  \Reduce_Value_1
 | |  |   \Number: 7
 | |  \CloseParen: 
 | +Reduce_FactorOp_1
 | |\Multiply: 
 | \Reduce_Factor_1
 |  +Reduce_Value_1
 |  |\Number: 5
 |  +Power: 
 |  \Reduce_Value_1
 |   \Number: 2
 +Reduce_ComparisonOp_5
 |\GreaterThan: 
 \Reduce_Expression_2
  \Reduce_Term_2
   \Reduce_Factor_2
    \Reduce_Value_1
     \Number: 0
";

			var parser = NewParser();
			var tokens = Token.GetTokens(Source);
			object result;

			try
			{
				result = parser.Parse(null, tokens);
			}
			catch
			{
				if (parser.SupportsTrace)
				{
					System.Diagnostics.Trace.WriteLine(parser.Trace);
				}

				throw;
			}

			Assert.That(Render(result), Is.EqualTo(Expected));
		}

		[Test]
		public void Trace()
		{
			const string Source = @"
Number: 5
Power:
Number: 2
GreaterThan:
Number: 0
";

			const string Trace = @"
State  | Type                 | Action
-------+----------------------+------------
     0 | Number               | Shift
    14 | Power                | <Value> -> Number
    12 | Power                | Shift
     7 | Number               | Shift
    14 | GreaterThan          | <Value> -> Number
    26 | GreaterThan          | <Factor> -> <Value> Power <Value>
    13 | GreaterThan          | <Term> -> <Factor>
     2 | GreaterThan          | <Expression> -> <Term>
     1 | GreaterThan          | Shift
    16 | Number               | <ComparisonOp> -> GreaterThan
     4 | Number               | Shift
    14 | EOF                  | <Value> -> Number
    12 | EOF                  | <Factor> -> <Value>
    13 | EOF                  | <Term> -> <Factor>
     2 | EOF                  | <Expression> -> <Term>
     9 | EOF                  | <Equation> -> <Expression> <ComparisonOp> <Expression>
    11 | EOF                  | Accept
";

			var parser = NewParser();

			if (_trace)
			{
				Assert.That(parser.SupportsTrace, Is.True);
				Assert.That(parser.Trace, Is.EqualTo(string.Empty));
			}

			parser.Parse(null, Token.GetTokens(Source));

			if (_trace)
			{
				Assert.That(parser.Trace, Is.EqualTo(Trace));
			}
			else
			{
				Assert.That(parser.SupportsTrace, Is.False);
				Assert.That(() => parser.Trace, Throws.Exception.InstanceOf<NotSupportedException>());
			}
		}

		#region Implementation

		IParser NewParser()
		{
			return ParserFactory.NewParser(typeof(T), delegate (string s, object[] args)
			{
				var result = new object[args.Length + 1];
				result[0] = s;

				if (args.Length > 0)
				{
					Array.Copy(args, 0, result, 1, args.Length);
				}

				return result;
			});
		}

		static string Render(object value)
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			Render(builder, value, string.Empty, true);
			return builder.ToString();
		}

		static void Render(StringBuilder builder, object value, string indent, bool isLast)
		{
			if (value == null)
			{
				builder.Append(indent);
				builder.AppendLine("<null>");
			}
			else if (value is object[])
			{
				Render(builder, (object[])value, indent, isLast);
			}
			else
			{
				Render(builder, (Token)value, indent, isLast);
			}
		}

		static void Render(StringBuilder builder, Token token, string indent, bool isLast)
		{
			builder.Append(indent);
			builder.Append(isLast ? '\\' : '+');
			builder.Append(token.TokenTypeName);
			builder.Append(": ");
			builder.AppendLine(token.Value);
		}

		static void Render(StringBuilder builder, object[] args, string indent, bool isLast)
		{
			builder.Append(indent);

			if (indent.Length > 0)
			{
				builder.Append(isLast ? '\\' : '+');
			}
			else
			{
				builder.Append('-');
			}

			builder.AppendLine((string)args[0]);

			if (args.Length > 0)
			{
				for (var i = 2; i < args.Length; i++)
				{
					Render(builder, args[i - 1], indent + (isLast ? ' ' : '|'), false);
				}

				Render(builder, args[args.Length - 1], indent + (isLast ? ' ' : '|'), true);
			}
		}

		readonly string _setName;
		readonly bool _embedTable;
		readonly bool _trace;

		#endregion
	}
}
