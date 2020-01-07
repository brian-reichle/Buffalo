// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using System.Text;
using Buffalo.Core;

namespace Buffalo.Main
{
	static class Templates
	{
		public static string GetTemplateText(GeneratorType type)
		{
			Stream stream;

			switch (type)
			{
				case GeneratorType.Lexer:
					stream = typeof(Templates).Assembly.GetManifestResourceStream("Buffalo.Main.Templates.LexerTemplate.l");
					break;

				case GeneratorType.Parser:
					stream = typeof(Templates).Assembly.GetManifestResourceStream("Buffalo.Main.Templates.ParserTemplate.y");
					break;

				default: return null;
			}

			using (stream)
			using (TextReader reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
