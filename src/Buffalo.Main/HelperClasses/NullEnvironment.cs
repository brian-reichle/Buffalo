// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core;

namespace Buffalo.Main
{
	sealed class NullEnvironment : ICodeGeneratorEnv
	{
		NullEnvironment()
		{
		}

		public static NullEnvironment Instance { get; } = new NullEnvironment();

		public string GetResourceName(string suffix) => null;
	}
}
