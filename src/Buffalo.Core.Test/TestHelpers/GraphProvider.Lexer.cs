// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Lexer;
using Buffalo.Core.Lexer.Configuration;
using Buffalo.TestResources;
using Moq;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Test
{
	static partial class GraphProvider
	{
		public static object LexerScanGraph()
		{
			return GenerateScanGraph(LexerTestFiles.Scanner().Config.ReadString());
		}

		public static object ParserScanGraph()
		{
			return GenerateScanGraph(LexerTestFiles.Parser().Config.ReadString());
		}

		static object GenerateScanGraph(string text)
		{
			var errorReporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			var parser = new ConfigParser(errorReporter.Object);
			var config = parser.Parse(new ConfigScanner(text));
			var state = config.States[0];

			var nfa = new Graph.Builder();
			var start = nfa.NewState(true, new NodeData(0, null));

			for (var i = 0; i < state.Rules.Count; i++)
			{
				var rule = state.Rules[i];
				var element = ReParser.Parse(rule.Regex.Text);

				element.GenerateNFA(nfa, start, nfa.NewState(false, new NodeData(null, i)));
			}

			return FATools.CreateDfa(nfa.Graph);
		}
	}
}
