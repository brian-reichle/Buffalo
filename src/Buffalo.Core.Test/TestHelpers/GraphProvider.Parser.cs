// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Parser;
using Buffalo.Core.Parser.Configuration;
using Buffalo.TestResources;
using Moq;

namespace Buffalo.Core.Test
{
	static partial class GraphProvider
	{
		public static object LexerParseGraph()
		{
			return GenerateParseGraph(ParserTestFiles.Scanner().Config.ReadString());
		}

		public static object ParserParseGraph()
		{
			return GenerateParseGraph(ParserTestFiles.Parser().Config.ReadString());
		}

		static object GenerateParseGraph(string text)
		{
			var errorReporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			var parser = new ConfigParser(errorReporter.Object);
			var config = parser.Parse(new ConfigScanner(text));
			SyntaxTreeDecorator.Decorate(config, errorReporter.Object);
			var parse = ParseGraph.ConstructGraph(config);
			return parse.Graph;
		}
	}
}
