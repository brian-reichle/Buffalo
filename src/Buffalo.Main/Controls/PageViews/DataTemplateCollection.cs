// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Buffalo.Main
{
	[DebuggerDisplay("Count = {Count}")]
	sealed class DataTemplateCollection : IList<DataTemplate>, IList
	{
		public DataTemplateCollection()
		{
			_list = new List<DataTemplate>();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public int Count => _list.Count;

		public DataTemplate this[int index]
		{
			get => _list[index];
			set => _list[index] = value;
		}

		public int IndexOf(DataTemplate item) => _list.IndexOf(item);
		public void Insert(int index, DataTemplate item) => _list.Insert(index, item);
		public void RemoveAt(int index) => _list.RemoveAt(index);
		public void Add(DataTemplate item) => _list.Add(item);
		public bool Remove(DataTemplate item) => _list.Remove(item);
		public void Clear() => _list.Clear();
		public bool Contains(DataTemplate item) => _list.Contains(item);
		public void CopyTo(DataTemplate[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		public IEnumerator<DataTemplate> GetEnumerator() => _list.GetEnumerator();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection<DataTemplate>.IsReadOnly => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IList.IsFixedSize => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IList.IsReadOnly => false;

		int IList.Add(object value)
		{
			Add((DataTemplate)value);
			return Count - 1;
		}

		object IList.this[int index]
		{
			[DebuggerStepThrough]
			get => this[index];
			set => this[index] = (DataTemplate)value;
		}

		bool IList.Contains(object value) => Contains((DataTemplate)value);
		int IList.IndexOf(object value) => IndexOf((DataTemplate)value);
		void IList.Insert(int index, object value) => Insert(index, (DataTemplate)value);
		void IList.Remove(object value) => Remove((DataTemplate)value);
		void ICollection.CopyTo(Array array, int index) => ((ICollection)_list).CopyTo(array, index);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;
		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly List<DataTemplate> _list;
	}
}
