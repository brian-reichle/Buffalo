// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core;

namespace Buffalo.Vsix
{
	sealed class GeneratorEnvironment : ICodeGeneratorEnv
	{
		public GeneratorEnvironment(string baseResourceName)
		{
			_baseResourceName = baseResourceName;
		}

		public string GetResourceName(string suffix) => _baseResourceName + suffix;

		readonly string _baseResourceName;
	}
}
