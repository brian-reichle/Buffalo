// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Buffalo.Core.Test
{
	[Serializable]
	[DebuggerDisplay("Type = {TokenTypeName}, Value = {Value}")]
	public sealed class Token
	{
		public static Token[] GetTokens(string inString)
		{
			var tokens = new List<Token>();
			using (var reader = new StringReader(inString))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					var index = line.IndexOf(':');

					if (index >= 0)
					{
						tokens.Add(new Token(
							line.Substring(0, index).Trim(),
							line.Substring(index + 1).Trim(),
							new CharPosX(),
							new CharPosX()));
					}
				}
			}

			tokens.Add(new Token(
				"EOF",
				string.Empty,
				new CharPosX(),
				new CharPosX()));

			return tokens.ToArray();
		}

		public Token(string tokenTypeName, string value, CharPosX fromPos, CharPosX toPos)
		{
			TokenTypeName = tokenTypeName;
			Value = value;
			FromPos = fromPos;
			ToPos = toPos;
		}

		public string TokenTypeName { get; private set; }
		public string Value { get; private set; }
		public CharPosX FromPos { get; private set; }
		public CharPosX ToPos { get; private set; }
	}
}
