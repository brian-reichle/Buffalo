// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Buffalo.Main
{
	sealed class RangeMap : IEnumerable<Range>
	{
		public void Clear()
		{
			_root = null;
		}

		public void Set(int start, int length)
		{
			if (length > 0)
			{
				Set(ref _root, start, start + length - 1);
			}
		}

		public void Unset(int start, int length)
		{
			if (length > 0)
			{
				Unset(ref _root, start, start + length - 1);
			}
		}

		public void Insert(int start, int length)
		{
			if (length > 0)
			{
				Insert(_root, start, length);
			}
		}

		public void Delete(int start, int length)
		{
			if (length > 0)
			{
				Delete(ref _root, start, start + length - 1, -1);
			}
		}

		public IEnumerator<Range> GetEnumerator(int from, int to)
		{
			var stack = new Stack<Node>();

			var cursor = _root;

			do
			{
				if (cursor != null)
				{
					stack.Push(cursor);
					cursor = from < cursor.Start ? cursor.Left : null;
				}
				else if (stack.Count == 0)
				{
					yield break;
				}
				else
				{
					cursor = stack.Pop();

					if (from <= cursor.End && to >= cursor.Start)
					{
						var rStart = Math.Max(from, cursor.Start);
						var rEnd = Math.Min(to, cursor.End);

						yield return new Range(rStart, rEnd - rStart + 1);
					}

					cursor = to > cursor.End ? cursor.Right : null;
				}
			}
			while (true);
		}

		static void Set(ref Node root, int start, int end)
		{
			if (root == null)
			{
				root = new Node()
				{
					Start = start,
					End = end,
				};
			}
			else if (end + 1 < root.Start)
			{
				Set(ref root.Left, start, end);
			}
			else if (root.End + 1 < start)
			{
				Set(ref root.Right, start, end);
			}
			else
			{
				if (start < root.Start)
				{
					root.Start = SetLeft(ref root.Left, start);
				}

				if (end > root.End - 1)
				{
					root.End = SetRight(ref root.Right, end);
				}
			}
		}

		static int SetLeft(ref Node root, int start)
		{
			if (root == null)
			{
				return start;
			}
			else if (start < root.Start)
			{
				var result = SetLeft(ref root.Left, start);
				root = root.Left;
				return result;
			}
			else if (start - 1 <= root.End)
			{
				var result = root.Start;
				root = root.Left;
				return result;
			}
			else
			{
				return start;
			}
		}

		static int SetRight(ref Node root, int end)
		{
			if (root == null)
			{
				return end;
			}
			else if (end > root.End)
			{
				var result = SetRight(ref root.Right, end);
				root = root.Right;
				return result;
			}
			else if (end + 1 <= root.Start)
			{
				var result = root.End;
				root = root.Right;
				return result;
			}
			else
			{
				return end;
			}
		}

		static void Unset(ref Node root, int start, int end)
		{
			if (root == null)
			{
				return;
			}
			else if (start > root.End)
			{
				Unset(ref root.Right, start, end);
			}
			else if (end < root.Start)
			{
				Unset(ref root.Left, start, end);
			}
			else if (start > root.Start)
			{
				if (end < root.End)
				{
					var newNode = new Node();
					newNode.Start = root.Start;
					newNode.End = start - 1;
					newNode.Left = root.Left;

					root.Left = newNode;
					root.Start = end + 1;
				}
				else
				{
					Unset(ref root.Right, start, end);
					root.End = start - 1;
				}
			}
			else
			{
				Unset(ref root.Left, start, end);

				if (end < root.End)
				{
					root.Start = end + 1;
				}
				else
				{
					Unset(ref root.Right, start, end);
					DeleteNode(ref root);
				}
			}
		}

		static void Insert(Node root, int start, int length)
		{
			if (root == null) return;

			if (start > root.End)
			{
				Insert(root.Right, start, length);
			}
			else if (start > root.Start)
			{
				root.End += length;
				Insert(root.Right, start, length);
			}
			else
			{
				Insert(root.Left, start, length);
				root.Start += length;
				root.End += length;
				Insert(root.Right, start, length);
			}
		}

		static int Delete(ref Node root, int start, int end, int newEnd)
		{
			if (root == null) return newEnd;

			if (start > root.End)
			{
				Delete(ref root.Right, start, end, -1);
			}
			else if (end < root.Start)
			{
				Delete(ref root.Left, start, end, -1);
				var len = end - start + 1;
				root.Start -= len;
				root.End -= len;
				Delete(ref root.Right, start, end, -1);
			}
			else if (start > root.Start)
			{
				if (end < root.End)
				{
					var len = end - start + 1;
					root.End -= len;
					Delete(ref root.Right, start, end, -1);
				}
				else
				{
					root.End = Delete(ref root.Right, start, end, start - 1);
				}
			}
			else
			{
				Delete(ref root.Left, start, end, -1);

				if (end < root.End)
				{
					Delete(ref root.Right, start, end, -1);

					if (newEnd > -1)
					{
						newEnd = root.End - end + start - 1;
						DeleteNode(ref root);
					}
					else
					{
						root.Start = start;
						root.End = root.End - end + start - 1;
					}
				}
				else
				{
					newEnd = Delete(ref root.Right, start, end, newEnd);
					DeleteNode(ref root);
				}
			}

			return newEnd;
		}

		static void DeleteNode(ref Node root)
		{
			if (root == null) return;

			if (root.Left == null)
			{
				root = root.Right;
			}
			else if (root.Right == null)
			{
				root = root.Left;
			}
			else
			{
				RotateLeft(ref root);
				DeleteNode(ref root.Left);
			}
		}

		static void RotateLeft(ref Node root)
		{
			var pivot = root.Right;
			root.Right = pivot.Left;
			pivot.Left = root;
			root = pivot;
		}

		Node _root;

		public IEnumerator<Range> GetEnumerator()
		{
			var stack = new Stack<Node>();

			var cursor = _root;

			do
			{
				if (cursor != null)
				{
					stack.Push(cursor);
					cursor = cursor.Left;
				}
				else if (stack.Count == 0)
				{
					yield break;
				}
				else
				{
					cursor = stack.Pop();
					yield return new Range(cursor.Start, cursor.End - cursor.Start + 1);
					cursor = cursor.Right;
				}
			}
			while (true);
		}

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[DebuggerDisplay("({Start})-({End})")]
		sealed class Node
		{
#pragma warning disable SA1401 // Fields must be private
			public Node Left;
			public Node Right;
#pragma warning restore SA1401 // Fields must be private

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Start { get; set; }
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int End { get; set; }
		}
	}
}
