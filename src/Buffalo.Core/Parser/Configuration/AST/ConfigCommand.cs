// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser.Configuration
{
	abstract class ConfigCommand
	{
		public Segment Segment { get; set; }
		public ConfigUsing Using { get; set; }

		public abstract void Apply(IConfigCommandVisitor visitor);
	}
}
