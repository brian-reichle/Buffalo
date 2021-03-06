// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser.Configuration
{
	interface IConfigCommandVisitor
	{
		void Visit(ConfigCommandNull nullValue);
		void Visit(ConfigCommandArg argValue);
		void Visit(ConfigCommandMethod method);
	}
}
