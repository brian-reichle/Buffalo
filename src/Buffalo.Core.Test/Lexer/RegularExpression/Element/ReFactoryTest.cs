// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;

namespace Buffalo.Core.Lexer.Test
{
	[TestFixture]
	public sealed class ReFactoryTest
	{
		[Test]
		public void Singleton()
		{
			var element = ReFactory.NewSingleton(CharSet.New('1'));
			Assert.That(element, Is.Not.EqualTo(null));
			Assert.That(element, Is.TypeOf(typeof(ReSingleton)));

			var singleton = (ReSingleton)element;
			Assert.That(singleton.Label, Is.EqualTo(CharSet.New('1')));
		}

		[Test]
		public void Concatenation_Empty()
		{
			var element = ReFactory.NewConcatenation(Array.Empty<ReElement>());
			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReEmptyString)));
		}

		[Test]
		public void Concatenation_Single()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewConcatenation(new ReElement[] { child });
			Assert.That(element, Is.SameAs(element));
		}

		[Test]
		public void Concatenation_Collapse()
		{
			var child1 = ReUtils.NewDummy('1');
			var child2 = ReUtils.NewDummy('2');
			var child3 = ReUtils.NewDummy('3');
			var child4 = ReUtils.NewDummy('4');
			var subConcat = new ReConcatenation(ImmutableArray.Create(child2, child3));

			var element = ReFactory.NewConcatenation(new ReElement[] { child1, subConcat, ReEmptyString.Instance, child4 });

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReConcatenation)));

			var concat = (ReConcatenation)element;
			Assert.That(concat.Elements, Is.EquivalentTo(new ReElement[] { child1, child2, child3, child4 }));
		}

		[Test]
		public void Concatenation_Broken()
		{
			var child1 = ReUtils.NewDummy('1');
			var child2 = ReUtils.NewDummy('2');
			var broke = ReEmptyLanguage.Instance;

			var element = ReFactory.NewConcatenation(ImmutableArray.Create(child1, broke, child2));

			Assert.That(element, Is.SameAs(broke));
		}

		[Test]
		public void Union_Empty()
		{
			var element = ReFactory.NewUnion(Array.Empty<ReElement>());
			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReEmptyLanguage)));
		}

		[Test]
		public void Union_Single()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewUnion(ImmutableArray.Create(child));
			Assert.That(element, Is.SameAs(child));
		}

		[Test]
		public void Union_Collapse()
		{
			var child1 = ReUtils.NewDummy('1');
			var child2 = ReUtils.NewDummy('2');
			var child3 = ReUtils.NewDummy('3');
			var child4 = ReUtils.NewDummy('4');

			var subUnion = new ReUnion(ImmutableArray.Create(child2, child3));

			var element = ReFactory.NewUnion(new ReElement[] { child1, subUnion, ReEmptyLanguage.Instance, child4 });

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReUnion)));

			var union = (ReUnion)element;
			Assert.That(union.Elements, Is.EquivalentTo(new ReElement[] { child1, child2, child3, child4 }));
		}

		[Test]
		public void KleenStar_EmptyString()
		{
			var empty = ReEmptyString.Instance;
			var element = ReFactory.NewKleeneStar(empty);

			Assert.That(element, Is.SameAs(empty));
		}

		[Test]
		public void KleenStar_EmptyLanguage()
		{
			var element = ReFactory.NewKleeneStar(ReEmptyLanguage.Instance);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReEmptyLanguage)));
		}

		[Test]
		public void KleenStar_NonNested()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewKleeneStar(child);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReKleeneStar)));

			var star = (ReKleeneStar)element;
			Assert.That(star.Element, Is.SameAs(child));
		}

		[Test]
		public void KleeneStar_NestedKleenStar()
		{
			var child = ReUtils.NewDummy('1');
			var subStar = new ReKleeneStar(child);

			var element = ReFactory.NewKleeneStar(subStar);
			Assert.That(element, Is.SameAs(subStar));
		}

		[Test]
		public void Repetition_2()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewRepetition(child, 2, 2);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReConcatenation)));

			var concat = (ReConcatenation)element;
			Assert.That(concat.Elements, Is.EquivalentTo(new ReElement[] { child, child }));
		}

		[Test]
		public void Repetition_0To1()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewRepetition(child, 0, 1);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReUnion)));

			var elements = new List<ReElement>(((ReUnion)element).Elements);
			Assert.That(elements.Count, Is.EqualTo(2));
			Assert.That(elements[0], Is.SameAs(child));
			Assert.That(elements[1], Is.Not.Null);
			Assert.That(elements[1], Is.TypeOf(typeof(ReEmptyString)));
		}

		[Test]
		public void Repetition_1To2()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewRepetition(child, 1, 2);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReConcatenation)));

			var concatElements = new List<ReElement>(((ReConcatenation)element).Elements);
			Assert.That(concatElements.Count, Is.EqualTo(2));
			Assert.That(concatElements[0], Is.SameAs(child));
			Assert.That(concatElements[1], Is.Not.Null);
			Assert.That(concatElements[1], Is.TypeOf(typeof(ReUnion)));

			var unionElements = new List<ReElement>(((ReUnion)concatElements[1]).Elements);
			Assert.That(unionElements.Count, Is.EqualTo(2));
			Assert.That(unionElements[0], Is.SameAs(child));
			Assert.That(unionElements[1], Is.Not.Null);
			Assert.That(unionElements[1], Is.TypeOf(typeof(ReEmptyString)));
		}

		[Test]
		public void Repetition_0ToU()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewRepetition(child, 0, null);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReKleeneStar)));

			var star = (ReKleeneStar)element;
			Assert.That(star.Element, Is.SameAs(child));
		}

		[Test]
		public void Repetition_1ToU()
		{
			var child = ReUtils.NewDummy('1');

			var element = ReFactory.NewRepetition(child, 1, null);

			Assert.That(element, Is.Not.Null);
			Assert.That(element, Is.TypeOf(typeof(ReConcatenation)));

			var concatElements = new List<ReElement>(((ReConcatenation)element).Elements);
			Assert.That(concatElements.Count, Is.EqualTo(2));
			Assert.That(concatElements[0], Is.SameAs(child));
			Assert.That(concatElements[1], Is.Not.Null);
			Assert.That(concatElements[1], Is.TypeOf(typeof(ReKleeneStar)));

			var star = (ReKleeneStar)concatElements[1];
			Assert.That(star.Element, Is.SameAs(child));
		}
	}
}
