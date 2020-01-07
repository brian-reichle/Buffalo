// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Diagnostics;
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser.Configuration
{
	[DebuggerDisplay("Type = {Type}, Text = {Text}")]
	sealed class ConfigToken : IToken
	{
		public ConfigToken(ConfigTokenType type, CharPos fromPos, CharPos toPos, string text)
		{
			Type = type;
			FromPos = fromPos;
			ToPos = toPos;
			Text = text;
		}

		public string Text { get; }
		public ConfigTokenType Type { get; }
		public CharPos FromPos { get; }
		public CharPos ToPos { get; }

		SettingTokenType IToken.Type
		{
			get
			{
				switch (Type)
				{
					case ConfigTokenType.Label: return SettingTokenType.Label;
					case ConfigTokenType.String: return SettingTokenType.String;
					default: return SettingTokenType.Invalid;
				}
			}
		}
	}
}
