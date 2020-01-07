// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using Buffalo.Core.Lexer;
using Buffalo.Core.Lexer.Configuration;

namespace Buffalo.Core.Test
{
	static partial class Renderer
	{
		public static string Render(ReElement element)
		{
			var builder = new StringBuilder();
			Render(builder, 0, element);
			return builder.ToString();
		}

		static void Render(StringBuilder builder, int indent, IEnumerable<ReElement> elements)
		{
			builder.AppendLine("new IReElement[]");
			builder.Append(' ', indent);
			builder.AppendLine("{");

			foreach (var element in elements)
			{
				builder.Append(' ', indent + 1);
				Render(builder, indent + 1, element);
				builder.AppendLine(",");
			}

			builder.Append(' ', indent);
			builder.Append("}");
		}

		static void Render(StringBuilder builder, int indent, ReElement element)
		{
			switch (element.Kind)
			{
				case ReElementKind.EmptyLanguage:
					RenderReEmptyLanguage(builder);
					break;

				case ReElementKind.EmptyString:
					RenderReEmptyString(builder);
					break;

				case ReElementKind.Singleton:
					RenderReSingleton(builder, indent, (ReSingleton)element);
					break;

				case ReElementKind.KleenStar:
					RenderReKleenStar(builder, indent, (ReKleeneStar)element);
					break;

				case ReElementKind.Concatenation:
					RenderReConcatenation(builder, indent, (ReConcatenation)element);
					break;

				case ReElementKind.Union:
					RenderReUnion(builder, indent, (ReUnion)element);
					break;

				default:
					throw new InvalidOperationException("and what am i supposed to do with a '" + element.Kind + "'?");
			}
		}

		static void RenderReSingleton(StringBuilder builder, int indent, ReSingleton element)
		{
			builder.Append("new ReSingleton(");
			Render(builder, indent, element.Label);
			builder.Append(")");
		}

		static void RenderReUnion(StringBuilder builder, int indent, ReUnion element)
		{
			builder.Append("new ReUnion(");
			Render(builder, indent, element.Elements);
			builder.Append(")");
		}

		static void RenderReKleenStar(StringBuilder builder, int indent, ReKleeneStar element)
		{
			builder.AppendLine("new ReKleeneStar");
			builder.Append(' ', indent);
			builder.AppendLine("(");
			builder.Append(' ', indent + 1);
			Render(builder, indent + 1, element.Element);
			builder.AppendLine();
			builder.Append(' ', indent);
			builder.Append(")");
		}

		static void RenderReConcatenation(StringBuilder builder, int indent, ReConcatenation element)
		{
			builder.Append("new ReConcatenation(");
			Render(builder, indent, element.Elements);
			builder.Append(")");
		}

		static void RenderReEmptyLanguage(StringBuilder builder)
		{
			builder.Append("new ReEmptyLanguage()");
		}

		static void RenderReEmptyString(StringBuilder builder)
		{
			builder.Append("new ReEmptyString()");
		}

		public static string Render(IEnumerable<CharSet> charSetList)
		{
			var builder = new StringBuilder();

			var list = new List<CharSet>(charSetList);
			list.Sort(delegate (CharSet c1, CharSet c2)
			{
				return StringComparer.Ordinal.Compare(c1.ToString(), c2.ToString());
			});

			foreach (var set in list)
			{
				builder.AppendLine(set.ToString());
			}

			return builder.ToString();
		}

		static void Render(StringBuilder builder, int indent, CharSet set)
		{
			builder.Append("CharSet.New(");

			var ranges = new List<CharRange>(set);

			switch (ranges.Count)
			{
				case 0:
					builder.Append("new CharRange[] { }");
					break;

				case 1:
					builder.Append('\'');
					AppendEscapedChar(builder, ranges[0].From);

					if (ranges[0].To != ranges[0].From)
					{
						builder.Append("', '");
						AppendEscapedChar(builder, ranges[0].To);
					}

					builder.Append("'");
					break;

				default:
					builder.AppendLine("new CharRange[]");
					builder.Append(' ', indent);
					builder.AppendLine("{");

					foreach (var range in ranges)
					{
						builder.Append(' ', indent + 1);
						Render(builder, range);
						builder.AppendLine(",");
					}

					builder.Append(' ', indent);
					builder.Append("}");
					break;
			}

			builder.Append(")");
		}

		static void Render(StringBuilder builder, CharRange range)
		{
			builder.Append("new CharRange('");
			AppendEscapedChar(builder, range.From);
			builder.Append("', '");
			AppendEscapedChar(builder, range.To);
			builder.Append("')");
		}

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
