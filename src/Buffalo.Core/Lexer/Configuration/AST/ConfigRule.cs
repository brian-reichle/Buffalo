// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Lexer.Configuration
{
	sealed class ConfigRule
	{
		public ConfigRule(ConfigToken regex, ConfigToken token)
		{
			Regex = regex;
			Token = token;
		}

		public ConfigToken Regex { get; }
		public ConfigToken Token { get; }
	}
}
