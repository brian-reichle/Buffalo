// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections;
using System.Collections.Generic;

namespace Buffalo.Core.Common
{
	interface IReadOnlyIndexedCollection<T> : IEnumerable<T>, ICollection
	{
		T this[int stateId] { get; }

		void CopyTo(T[] array, int index);
	}
}
