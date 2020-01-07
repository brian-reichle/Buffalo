// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using Buffalo.Core.Parser.Configuration;

namespace Buffalo.Core.Test
{
	static partial class Renderer
	{
		public static string Render(IEnumerable<ConfigToken> tokens)
		{
			var builder = new StringBuilder();
			Render(builder, 0, tokens);
			return builder.ToString();
		}

		static void Render(StringBuilder builder, int indent, IEnumerable<ConfigToken> tokens)
		{
			builder.AppendLine("new ConfigToken[]");
			builder.Append('\t', indent);
			builder.AppendLine("{");

			foreach (var token in tokens)
			{
				builder.Append('\t', indent + 1);
				Render(builder, token);
				builder.AppendLine(",");
			}

			builder.Append('\t', indent);
			builder.Append("}");
		}

		static void Render(StringBuilder builder, ConfigToken token)
		{
			if (token == null)
			{
				builder.Append("null");
			}
			else
			{
				builder.Append("new ConfigToken(");

				if (Enum.IsDefined(typeof(ConfigTokenType), token.Type))
				{
					builder.Append("ConfigTokenType.");
					builder.Append(token.Type.ToString());
				}
				else
				{
					builder.Append("(int)(");
					builder.Append((int)token.Type);
					builder.Append(')');
				}

				builder.Append(", ");
				Render(builder, token.FromPos);
				builder.Append(", ");
				Render(builder, token.ToPos);
				builder.Append(", ");
				AppendString(builder, token.Text);
				builder.Append(')');
			}
		}
	}
}
