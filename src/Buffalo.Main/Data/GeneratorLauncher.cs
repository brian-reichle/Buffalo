// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using Buffalo.Core;

namespace Buffalo.Main
{
	static class GeneratorLauncher
	{
		public static async Task GenerateAsync(Dispatcher dispatcher, ConfigPage page)
		{
			var manager = page.Manager;

			if (!manager.IsGenerating)
			{
				var tok = new GenerationToken(dispatcher, page);

				manager.IsGenerating = true;
				try
				{
					manager.TimeTaken = TimeSpan.Zero;
					manager.Notifications.Clear(page);
					manager.ProcessDurations.Clear();

					await GenerateAsync(tok);
					Update(tok, page);
				}
				finally
				{
					manager.IsGenerating = false;
					manager.TimeTaken = tok.Duration;
				}
			}
		}

		static Task GenerateAsync(GenerationToken tok)
		{
			return Task.Run(() => GenerateCore(tok));
		}

		static void GenerateCore(GenerationToken tok)
		{
			var generator = GeneratorFactory.NewGenerator(tok.GeneratorType);
			var start = DateTime.Now;

			try
			{
				using (var reader = new StringReader(tok.Config))
				{
					if (generator.Load(reader, tok, new Environment(tok.Filename)))
					{
						using (var writer = new StringWriter())
						{
							generator.Write(writer);
							tok.Results.Add(new CodeResultToken(writer.ToString(), ".g.cs"));
						}

						foreach (var file in generator.AdditionalFiles)
						{
							if (file.IsBinary)
							{
								tok.Results.Add(WriteBinary(file));
							}
							else
							{
								tok.Results.Add(WriteText(file));
							}
						}
					}
					else
					{
						tok.Results.Add(new CodeResultToken(string.Empty, ".g.cs"));
					}
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
			{
				tok.AddError(1, 1, 1, 1, "KA-BOOM!!!");
				tok.Results.Add(new CodeResultToken(ex.ToString(), ".g.cs"));
			}
#pragma warning restore CA1031 // Do not catch general exception types

			var end = DateTime.Now;
			tok.Duration = end - start;

			generator.ReportPerformance(tok);
		}

		static IResultToken WriteBinary(IAdditionalFile file)
		{
			using (var writer = new StringWriter())
			using (var stream = new HexStream(writer))
			{
				file.GenerateFileContent(stream);
				stream.Flush();

				return new CodeResultToken(writer.ToString(), file.Suffix);
			}
		}

		static IResultToken WriteText(IAdditionalFile file)
		{
			using (var writer = new StringWriter())
			{
				file.GenerateFileContent(writer);
				writer.Flush();

				return new CodeResultToken(writer.ToString(), file.Suffix);
			}
		}

		static void Update(GenerationToken tok, ConfigPage page)
		{
			var resultPages = page.ResultPages.ToArray();

			foreach (var result in tok.Results)
			{
				result.AddOrUpdatePage(page, resultPages);
			}

			RemoveRemaining(resultPages);

			page.DirtySince = null;
		}

		static void RemoveRemaining(ResultPage[] resultPages)
		{
			for (var i = 0; i < resultPages.Length; i++)
			{
				var resultPage = resultPages[i];

				if (resultPage != null)
				{
					resultPage.Close();
					resultPages[i] = null;
				}
			}
		}

		sealed class GenerationToken : IErrorReporter, IPerformanceReporter
		{
			public GenerationToken(Dispatcher dispatcher, ConfigPage page)
			{
				if (page == null) throw new ArgumentNullException(nameof(page));

				Config = page.ConfigText;
				Filename = page.FileName;
				GeneratorType = page.GeneratorType;
				Results = new List<IResultToken>();

				_dispatcher = dispatcher;
				_errReporter = page;
				_perfReporter = page.Manager;
			}

			public string Config { get; }
			public string Filename { get; }
			public TimeSpan Duration { get; set; }
			public GeneratorType GeneratorType { get; }
			public List<IResultToken> Results { get; }

			public void AddError(int fromLine, int fromChar, int toLine, int toChar, string text)
			{
				Action<int, int, int, int, string> target = _errReporter.AddError;
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, target, fromLine, fromChar, toLine, toChar, text);
			}

			public void AddWarning(int fromLine, int fromChar, int toLine, int toChar, string text)
			{
				Action<int, int, int, int, string> target = _errReporter.AddWarning;
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, target, fromLine, fromChar, toLine, toChar, text);
			}

			public void AddPerformanceMetric(string name, TimeSpan span)
			{
				Action<string, TimeSpan> target = _perfReporter.AddPerformanceMetric;
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, target, name, span);
			}

			readonly Dispatcher _dispatcher;
			readonly IErrorReporter _errReporter;
			readonly IPerformanceReporter _perfReporter;
		}
	}
}
