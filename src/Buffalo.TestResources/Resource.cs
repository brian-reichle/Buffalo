// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Buffalo.TestResources
{
	[DebuggerDisplay("{ResourceName}")]
	public class Resource
	{
		public Resource(string resourceName)
			: this(Assembly.GetCallingAssembly(), resourceName)
		{
		}

		public Resource(Assembly assembly, string resourceName)
		{
			_assembly = assembly;
			ResourceName = resourceName;
		}

		public string ResourceName { get; }
		public Stream CreateStream() => _assembly.GetManifestResourceStream(ResourceName);
		public TextReader CreateTextReader() => new StreamReader(CreateStream());

		public byte[] ReadBytes()
		{
			using (var stream = CreateStream())
			{
				var result = new byte[stream.Length];
				stream.Read(result, 0, result.Length);
				return result;
			}
		}

		public string ReadString()
		{
			using (var reader = CreateTextReader())
			{
				return reader.ReadToEnd();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Assembly _assembly;
	}
}
