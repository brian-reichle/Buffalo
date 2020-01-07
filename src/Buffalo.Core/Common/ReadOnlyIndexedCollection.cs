// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	[DebuggerDisplay("Count = {Count}")]
	sealed class ReadOnlyIndexedCollection<T> : IReadOnlyIndexedCollection<T>
	{
		public ReadOnlyIndexedCollection(T[] list)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));
			_list = list;
		}

		public T this[int index] => _list[index];
		public void CopyTo(T[] array, int index) => Array.Copy(_list, 0, array, index, _list.Length);
		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_list).GetEnumerator();

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		void ICollection.CopyTo(Array array, int index) => _list.CopyTo(array, index);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public int Count => _list.Length;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => _list.SyncRoot;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly T[] _list;
	}
}
