// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Buffalo.Core.Common;
using Buffalo.TestResources;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Buffalo.Core.Test
{
	static class GeneratorRunner
	{
		public static void Run<T>(ResourceSet set, IErrorReporter reporter, ICodeGeneratorEnv environment)
			where T : ICodeGenerator, new()
		{
			using (var writer = new StringWriter())
			using (var reader = set.Config.CreateTextReader())
			{
				ICodeGenerator generator = new T();
				generator.Load(reader, reporter, environment);
				generator.Write(writer);

				var files = new List<IAdditionalFile>(generator.AdditionalFiles);
				Assert.That(files.Count, Is.EqualTo(set.AdditionalFiles.Count));

				var prefxLen = set.Config.ResourceName.LastIndexOf('.');

				for (var i = 0; i < set.AdditionalFiles.Count; i++)
				{
					var res = set.AdditionalFiles[i];
					var suffix = res.ResourceName.Substring(prefxLen);
					Constraint constraint;

					if (res.IsBinary)
					{
						constraint = new FileBinaryConstraint(suffix, res);
					}
					else
					{
						constraint = new FileTextConstraint(suffix, res);
					}

					Assert.That(files[i], constraint, "File: {0}", set.AdditionalFiles[i].ResourceName);
				}

				var actual = writer.ToString();
				var expected = set.Code.ReadString();

				try
				{
					Assert.That(writer.ToString(), Is.EqualTo(expected));
				}
				catch
				{
					Trace.WriteLine(actual);
					throw;
				}
			}
		}

		public static void Run<T>(Resource resource, IErrorReporter reporter, ICodeGeneratorEnv environment)
			where T : ICodeGenerator, new()
		{
			using (var reader = resource.CreateTextReader())
			{
				var reporterWrapper = new ReporterWrapper(reporter);

				var generator = new T();
				generator.Load(reader, reporterWrapper, environment);

				if (!reporterWrapper.HasError)
				{
					using (TextWriter writer = new StringWriter())
					{
						generator.Write(writer);
					}

					foreach (var file in generator.AdditionalFiles)
					{
						using (Stream stream = new MemoryStream())
						{
							file.GenerateFileContent(stream);
						}
					}
				}
			}
		}
	}
}
