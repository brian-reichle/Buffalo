// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using Buffalo.Core;
using Microsoft.Win32;

namespace Buffalo.Main
{
	static class FileDialogHelper
	{
		public static void SetupFileDialog(GeneratorType? type, FileDialog dialog)
		{
			dialog.Filter = "Buffalo Config Files|*.l;*.y|Buffalo Lexer Config Files|*.l|Buffalo Parser Config Files|*.y";

			if (!type.HasValue)
			{
				dialog.FilterIndex = 1;
			}
			else
			{
				switch (type.Value)
				{
					case GeneratorType.Lexer:
						dialog.FilterIndex = 2;
						break;

					case GeneratorType.Parser:
						dialog.FilterIndex = 3;
						break;
				}
			}
		}

		public static GeneratorType? GetSelectedGeneratorType(FileDialog dialog)
		{
			switch (dialog.FilterIndex)
			{
				case 1:
					switch (Path.GetExtension(dialog.FileName))
					{
						case ".l": return GeneratorType.Lexer;
						case ".y": return GeneratorType.Parser;
						default: return null;
					}

				case 2: return GeneratorType.Lexer;
				case 3: return GeneratorType.Parser;
				default: return null;
			}
		}
	}
}
