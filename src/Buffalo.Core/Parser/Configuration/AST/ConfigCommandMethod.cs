// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;

namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigCommandMethod : ConfigCommand
	{
		public ConfigCommandMethod()
		{
			Arguments = new List<ConfigCommand>();
		}

		public ConfigToken Name { get; set; }
		public List<ConfigCommand> Arguments { get; }

		public override void Apply(IConfigCommandVisitor visitor) => visitor.Visit(this);
	}
}
