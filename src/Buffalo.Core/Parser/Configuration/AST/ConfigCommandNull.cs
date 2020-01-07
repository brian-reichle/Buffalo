// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser.Configuration
{
	sealed class ConfigCommandNull : ConfigCommand
	{
		public ConfigToken Value { get; set; }

		public override void Apply(IConfigCommandVisitor visitor) => visitor.Visit(this);
	}
}
