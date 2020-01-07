// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Diagnostics;
using System.Reflection;

namespace Buffalo.TestResources
{
	[DebuggerDisplay("{ResourceName} (IsBinary = {IsBinary})")]
	public sealed class TypedResource : Resource
	{
		public TypedResource(string resourceName, bool isBinary)
			: this(Assembly.GetCallingAssembly(), resourceName, isBinary)
		{
		}

		public TypedResource(Assembly assembly, string resourceName, bool isBinary)
			: base(assembly, resourceName)
		{
			IsBinary = isBinary;
		}

		public bool IsBinary { get; }
	}
}
