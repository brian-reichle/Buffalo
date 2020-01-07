// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class Environment : ICodeGeneratorEnv
	{
		public Environment(string baseName)
		{
			_baseName = baseName;
		}

		public string GetResourceName(string suffix) => Path.ChangeExtension(_baseName, suffix);

		readonly string _baseName;
	}
}
