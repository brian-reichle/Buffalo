// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using System.Text;
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class ManagerFileOperations
	{
		public static void Open(GenerationManager manager, GeneratorType? type, string fileName)
		{
			var page = new ConfigPage(manager);

			using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (TextReader reader = new StreamReader(stream, true))
			{
				page.ConfigText = reader.ReadToEnd();
				page.Path = Path.GetDirectoryName(fileName);
				page.FileName = Path.GetFileName(fileName);
			}

			if (type.HasValue)
			{
				page.GeneratorType = type.Value;
			}

			manager.Pages.Add(page);
			manager.CurrentPage = page;
		}

		public static void Save(ConfigPage page, string fileName)
		{
			using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			using (TextWriter writer = new StreamWriter(stream, Encoding.UTF8))
			{
				writer.Write(page.ConfigText);
			}
		}

		public static void New(GenerationManager manager, GeneratorType type, string fileName)
		{
			var page = manager.NewPage(type, fileName);

			var template = Templates.GetTemplateText(type);

			if (!string.IsNullOrEmpty(template))
			{
				page.ConfigText = template;
			}
		}
	}
}
