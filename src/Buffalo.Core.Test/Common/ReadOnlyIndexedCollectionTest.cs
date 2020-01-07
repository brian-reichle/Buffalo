// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public class ReadOnlyIndexedCollectionTest
	{
		[Test]
		public void Count()
		{
			Assert.That(new ReadOnlyIndexedCollection<int>(System.Array.Empty<int>()).Count, Is.EqualTo(0));
			Assert.That(new ReadOnlyIndexedCollection<int>(new int[] { 1, 6, 2, 5 }).Count, Is.EqualTo(4));
		}

		[Test]
		public void Index()
		{
			var collection = new ReadOnlyIndexedCollection<int>(new int[] { 1, 6, 2, 5 });

			Assert.That(collection[0], Is.EqualTo(1));
			Assert.That(collection[1], Is.EqualTo(6));
			Assert.That(collection[2], Is.EqualTo(2));
			Assert.That(collection[3], Is.EqualTo(5));
		}

		[Test]
		public void Enumerator()
		{
			var list = new List<int>(new ReadOnlyIndexedCollection<int>(new int[] { 1, 6, 2, 5 }));

			Assert.That(list[0], Is.EqualTo(1));
			Assert.That(list[1], Is.EqualTo(6));
			Assert.That(list[2], Is.EqualTo(2));
			Assert.That(list[3], Is.EqualTo(5));
		}
	}
}
