// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Test;

namespace Buffalo.Core.Parser.Test
{
	interface IParser
	{
		object Parse(string entry, Token[] tokens);
		bool SupportsTrace { get; }
		string Trace { get; }
	}
}
