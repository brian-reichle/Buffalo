// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Buffalo.Core.Lexer
{
	static class ReFactory
	{
		public static ReElement NewSingleton(CharSet value) => new ReSingleton(value);

		public static ReElement NewConcatenation(IReadOnlyList<ReElement> elements)
		{
			if (elements == null) throw new ArgumentNullException(nameof(elements));

			switch (elements.Count)
			{
				case 0: return ReEmptyString.Instance;
				case 1: return elements[0];
			}

			var builder = ImmutableArray.CreateBuilder<ReElement>(elements.Count);

			foreach (var element in elements)
			{
				switch (element.Kind)
				{
					case ReElementKind.Concatenation:
						builder.AddRange(((ReConcatenation)element).Elements);
						break;

					case ReElementKind.EmptyLanguage:
						return element;

					case ReElementKind.EmptyString:
						break;

					default:
						builder.Add(element);
						break;
				}
			}

			switch (builder.Count)
			{
				case 0: return ReEmptyString.Instance;
				case 1: return builder[0];
				default: return new ReConcatenation(builder.ToImmutable());
			}
		}

		public static ReElement NewUnion(IReadOnlyList<ReElement> elements)
		{
			if (elements == null) throw new ArgumentNullException(nameof(elements));

			switch (elements.Count)
			{
				case 0: return ReEmptyLanguage.Instance;
				case 1: return elements[0];
			}

			var builder = ImmutableArray.CreateBuilder<ReElement>(elements.Count);

			foreach (var element in elements)
			{
				switch (element.Kind)
				{
					case ReElementKind.Union:
						builder.AddRange(((ReUnion)element).Elements);
						break;

					case ReElementKind.EmptyLanguage:
						break;

					default:
						builder.Add(element);
						break;
				}
			}

			switch (builder.Count)
			{
				case 0: return ReEmptyLanguage.Instance;
				case 1: return builder[0];
				default: return new ReUnion(builder.ToImmutable());
			}
		}

		public static ReElement NewKleeneStar(ReElement element)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));

			switch (element.Kind)
			{
				case ReElementKind.KleenStar:
				case ReElementKind.EmptyLanguage:
				case ReElementKind.EmptyString:
					return element;

				default:
					return new ReKleeneStar(element);
			}
		}

		public static ReElement NewRepetition(ReElement element, int min, int? max)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));
			if (max.HasValue && min > max.Value) throw new ArgumentException("min cannot be greater than max", nameof(min));

			switch (element.Kind)
			{
				case ReElementKind.EmptyString:
				case ReElementKind.EmptyLanguage:
				case ReElementKind.KleenStar:
					return element;
			}

			if (element.MatchesEmptyString)
			{
				min = 0;
			}

			var builder = ImmutableArray.CreateBuilder<ReElement>();

			for (var i = 0; i < min; i++)
			{
				builder.Add(element);
			}

			if (max.HasValue)
			{
				var optionalElement = new ReUnion(
					ImmutableArray.Create(
						element,
						ReEmptyString.Instance));

				for (var i = min; i < max; i++)
				{
					builder.Add(optionalElement);
				}
			}
			else
			{
				builder.Add(new ReKleeneStar(element));
			}

			switch (builder.Count)
			{
				case 0: return ReEmptyString.Instance;
				case 1: return builder[0];
				default: return new ReConcatenation(builder.ToImmutable());
			}
		}
	}
}
