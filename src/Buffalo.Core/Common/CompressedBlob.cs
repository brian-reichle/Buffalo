// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	sealed class CompressedBlob : IList<int>, IList
	{
		public static CompressedBlob Compress(Compression method, ElementSizeStrategy elementSize, IList<int> uncompressedBlob)
		{
			switch (method)
			{
				case Compression.None:
					return new CompressedBlob(Compression.None, elementSize, elementSize, uncompressedBlob);

				case Compression.CTB:
					return new CompressedBlob(Compression.CTB, elementSize, U8SizeStrategy.Instance, Collapse_CTB(uncompressedBlob).ToArray());

				case Compression.Simple:
					return new CompressedBlob(Compression.Simple, elementSize, elementSize, Collapse_Simple(elementSize.MaxValue, uncompressedBlob).ToArray());

				case Compression.Auto:
					var tmp = Collapse_Simple(elementSize.MaxValue, uncompressedBlob);

					if (tmp.Count < uncompressedBlob.Count)
					{
						return new CompressedBlob(Compression.Simple, elementSize, elementSize, tmp);
					}
					else
					{
						return new CompressedBlob(Compression.None, elementSize, elementSize, uncompressedBlob);
					}

				default:
					throw new InvalidOperationException("Unrecognised compression method");
			}
		}

		public Compression Method { get; }
		public ElementSizeStrategy ElementSize { get; }
		public ElementSizeStrategy BlobSize { get; }

		public int Count => _blob.Length;
		public int Bytes => BlobSize.Size(_blob.Length);
		public int this[int index] => _blob[index];

		public void CopyBytesTo(byte[] dest, int sourceOffset, int count)
			=> BlobSize.CopyBytes(_blob, sourceOffset, dest, 0, count);

		public void CopyTo(int[] array, int arrayIndex) => Array.Copy(_blob, 0, array, arrayIndex, _blob.Length);

		CompressedBlob(Compression method, ElementSizeStrategy elementSize, ElementSizeStrategy blobSize, IList<int> blob)
		{
			Method = method;
			ElementSize = elementSize;
			BlobSize = blobSize;
			_blob = new int[blob.Count];
			blob.CopyTo(_blob, 0);
		}

		static List<int> Collapse_Simple(int escape, IList<int> expanded)
		{
			var result = new List<int>();
			result.Add(expanded.Count);
			result.Add(escape);

			var value = 0;
			var count = 0;

			for (var i = 0; i < expanded.Count; i++)
			{
				if (expanded[i] == value)
				{
					count++;
				}
				else
				{
					if (count > 0)
					{
						if (value == escape || count > 3)
						{
							result.Add(escape);
							result.Add(count);
							result.Add(value);
						}
						else
						{
							for (var j = 0; j < count; j++)
							{
								result.Add(value);
							}
						}
					}

					value = expanded[i];
					count = 1;
				}
			}

			if (value == escape || count > 3)
			{
				result.Add(escape);
				result.Add(count);
				result.Add(value);
			}
			else
			{
				for (var j = 0; j < count; j++)
				{
					result.Add(value);
				}
			}

			return result;
		}

		static List<int> Collapse_CTB(IList<int> uncompressedBlob)
		{
			var result = new List<int>();
			AddCTBBytes(result, uncompressedBlob.Count, false);

			if (uncompressedBlob.Count > 0)
			{
				var value = uncompressedBlob[0];
				var count = 1;

				for (var i = 1; i < uncompressedBlob.Count; i++)
				{
					if (uncompressedBlob[i] == value)
					{
						count++;
					}
					else
					{
						if (count > 1)
						{
							AddCTBBytes(result, count, true);
						}

						AddCTBBytes(result, value, false);

						count = 1;
						value = uncompressedBlob[i];
					}
				}

				if (value != 0)
				{
					if (count > 1)
					{
						AddCTBBytes(result, count, true);
					}

					AddCTBBytes(result, value, false);
				}
			}

			return result;
		}

		static void AddCTBBytes(List<int> target, int value, bool isRepitition)
		{
			const int READ_AGAIN = 0x80;
			const int REPITITION = 0x40;

			if (value == 0)
			{
				target.Add(isRepitition ? REPITITION : 0);
			}
			else
			{
				var bytes = new int[5];
				var count = 0;

				do
				{
					bytes[count] = value & 0x7F;
					value = unchecked((int)((uint)value >> 7));
					count++;
				}
				while (value != 0);

				if ((bytes[count - 1] & REPITITION) != 0)
				{
					count++;
				}

				if (isRepitition)
				{
					bytes[count - 1] |= REPITITION;
				}

				while (count > 1)
				{
					count--;
					target.Add(READ_AGAIN | bytes[count]);
				}

				target.Add(bytes[0]);
			}
		}

		int IList<int>.IndexOf(int item) => ((IList<int>)_blob).IndexOf(item);
		void IList<int>.Insert(int index, int item) => throw new NotSupportedException();
		void IList<int>.RemoveAt(int index) => throw new NotSupportedException();

		int IList<int>.this[int index]
		{
			[DebuggerStepThrough]
			get => _blob[index];
			set => throw new NotSupportedException();
		}

		void ICollection<int>.Add(int item) => throw new NotSupportedException();
		void ICollection<int>.Clear() => throw new NotSupportedException();
		bool ICollection<int>.Contains(int item) => ((ICollection<int>)_blob).Contains(item);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection<int>.IsReadOnly => true;
		bool ICollection<int>.Remove(int item) => throw new NotSupportedException();

		public IEnumerator<int> GetEnumerator() => ((ICollection<int>)_blob).GetEnumerator();

		int IList.Add(object value) => throw new NotSupportedException();
		void IList.Clear() => throw new NotSupportedException();
		bool IList.Contains(object value) => ((IList)_blob).Contains(value);
		int IList.IndexOf(object value) => ((IList)_blob).IndexOf(value);
		void IList.Insert(int index, object value) => throw new NotSupportedException();
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IList.IsFixedSize => true;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IList.IsReadOnly => true;
		void IList.Remove(object value) => throw new NotSupportedException();
		void IList.RemoveAt(int index) => throw new NotSupportedException();

		object IList.this[int index]
		{
			[DebuggerStepThrough]
			get => _blob[index];
			set => throw new NotSupportedException();
		}

		void ICollection.CopyTo(Array array, int index) => Array.Copy(_blob, 0, array, index, _blob.Length);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => _blob.SyncRoot;
		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly int[] _blob;
	}
}
