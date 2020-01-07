// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;
using System.Text;
using Buffalo.Core;

namespace Buffalo.Vsix
{
	abstract class Generator : BaseCodeGeneratorWithSite
	{
		protected Generator(GeneratorType type)
		{
			_type = type;
		}

		protected override string DefaultExtension => ".g.cs";

		protected sealed override byte[] GenerateBytes(string sourceFileContent)
		{
			var result = string.Empty;

			try
			{
				var generator = GeneratorFactory.NewGenerator(_type);
				bool isLoadded;

				using (TextReader reader = new StringReader(sourceFileContent))
				{
					var reporter = new ErrorReporter(this);
					ICodeGeneratorEnv environment = new GeneratorEnvironment(CalculateBaseResourceName());
					isLoadded = generator.Load(reader, reporter, environment);
				}

				if (isLoadded)
				{
					using (TextWriter writer = new StringWriter())
					{
						generator.Write(writer);
						result = writer.ToString();
					}

					foreach (var file in generator.AdditionalFiles)
					{
						byte[] blob;

						using (var stream = new MemoryStream())
						{
							if (file.IsBinary)
							{
								file.GenerateFileContent(stream);
								stream.Flush();
							}
							else
							{
								using (var writer = new StreamWriter(stream))
								{
									file.GenerateFileContent(writer);
									writer.Flush();
								}
							}

							blob = stream.ToArray();
						}

						var action = GetBuildAction(file);
						WriteAdditionalChild(file.Suffix, blob, action);
					}
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
			{
				var builder = new StringBuilder();
				builder.AppendLine("#error KA-BOOM!!!");
				AppendException(builder, ex);
				result = builder.ToString();
			}
#pragma warning restore CA1031 // Do not catch general exception types

			return Encoding.UTF8.GetBytes(result);
		}

		static BuildAction GetBuildAction(IAdditionalFile file)
		{
			BuildAction action;

			switch (file.Type)
			{
				case AdditionalFileType.Code:
					action = BuildAction.Compile;
					break;

				case AdditionalFileType.EmbeddedResource:
					action = BuildAction.EmbeddedResource;
					break;

				default:
					action = BuildAction.None;
					break;
			}

			return action;
		}

		static void AppendException(StringBuilder builder, Exception ex)
		{
			if (ex.InnerException != null)
			{
				AppendException(builder, ex.InnerException);
				builder.AppendLine("//");
				builder.AppendLine("// Re-thrown as:");
			}

			foreach (var line in ex.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None))
			{
				builder.Append("// ");
				builder.AppendLine(line);
			}
		}

		sealed class ErrorReporter : IErrorReporter
		{
			public ErrorReporter(Generator gen)
			{
				_gen = gen;
			}

			public void AddError(int fromLine, int fromChar, int toLine, int toChar, string text)
				=> _gen.GeneratorError(false, text, fromLine - 1, fromChar - 1);

			public void AddWarning(int fromLine, int fromChar, int toLine, int toChar, string text)
				=> _gen.GeneratorError(true, text, fromLine - 1, fromChar - 1);

			readonly Generator _gen;
		}

		readonly GeneratorType _type;
	}
}
