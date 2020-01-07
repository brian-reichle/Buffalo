// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace Buffalo.Core.Common.Test
{
	class FARenderer<TNode, TTransition>
	{
		public string Render(Graph<TNode, TTransition>.State initialState)
			=> Render(new Graph<TNode, TTransition>.State[] { initialState }, 0);

		public string Render(Graph<TNode, TTransition> graph)
			=> Render(graph.StartStates, 0);

		public string Render(Graph<TNode, TTransition>.State initialState, int width)
			=> Render(new Graph<TNode, TTransition>.State[] { initialState }, width);

		public string Render(Graph<TNode, TTransition> graph, int width)
			=> Render(graph.StartStates, width);

		public string Render(IEnumerable<Graph<TNode, TTransition>.State> initialStates, int width)
		{
			var visited = new Dictionary<Graph<TNode, TTransition>.State, bool>();
			var queue = new Queue<Graph<TNode, TTransition>.State>();
			var builder = new StringBuilder();

			foreach (var state in initialStates)
			{
				queue.Enqueue(state);
			}

			while (queue.Count > 0)
			{
				var state = queue.Dequeue();
				if (visited.ContainsKey(state)) continue;
				visited.Add(state, true);

				var fromNodeText = FormatState(state).Split(new string[] { "\r\n" }, StringSplitOptions.None);
				var fromNodeWidth = Math.Max(width, GetWidth(fromNodeText));

				foreach (var transition in state.ToTransitions)
				{
					var transitionText = FormatTransition(transition).Split(new string[] { "\r\n" }, StringSplitOptions.None);
					var transitionWidth = GetWidth(transitionText);

					var toNodeText = FormatState(transition.ToState).Split(new string[] { "\r\n" }, StringSplitOptions.None);
					var toNodeWidth = Math.Max(width, GetWidth(toNodeText));

					var lines = Math.Max(transitionText.Length, Math.Max(fromNodeText.Length, toNodeText.Length));

					builder.Append(fromNodeText[0].PadRight(fromNodeWidth, ' '));
					builder.Append(" -- ");
					builder.Append(transitionText[0].PadRight(transitionWidth, ' '));
					builder.Append(" --> ");
					builder.Append(toNodeText[0].PadRight(toNodeWidth, ' '));
					builder.AppendLine();

					for (var i = 1; i < lines; i++)
					{
						if (i < fromNodeText.Length)
						{
							builder.Append(fromNodeText[i].PadRight(fromNodeWidth, ' '));
						}
						else
						{
							builder.Append(' ', fromNodeWidth);
						}

						builder.Append(' ', 4);

						if (i < transitionText.Length)
						{
							builder.Append(transitionText[i].PadRight(transitionWidth, ' '));
						}
						else
						{
							builder.Append(' ', transitionWidth);
						}

						builder.Append(' ', 5);

						if (i < toNodeText.Length)
						{
							builder.Append(toNodeText[i].PadRight(toNodeWidth, ' '));
						}
						else
						{
							builder.Append(' ', toNodeWidth);
						}

						builder.AppendLine();
					}

					queue.Enqueue(transition.ToState);
				}
			}

			return builder.ToString();
		}

		protected virtual string FormatState(Graph<TNode, TTransition>.State state)
		{
			var builder = new StringBuilder();
			builder.Append(GetID(state));

			if (!Equals(state.Label, default(TNode)))
			{
				builder.Append(state.IsStartState ? " (S:" : " (L:");
				builder.Append(state.Label);
				builder.Append(")");
			}
			else if (state.IsStartState)
			{
				builder.Append(" (S)");
			}

			return builder.ToString();
		}

		protected virtual string FormatTransition(Graph<TNode, TTransition>.Transition transition)
		{
			if (transition.Label == null)
			{
				return "e";
			}
			else
			{
				return transition.Label.ToString();
			}
		}

		protected int GetID(Graph<TNode, TTransition>.State state)
		{
			if (!_ids.TryGetValue(state, out var result))
			{
				_ids.Add(state, result = _ids.Count);
			}

			return result;
		}

		static int GetWidth(string[] lines)
		{
			var width = 0;

			foreach (var line in lines)
			{
				width = Math.Max(width, line.Length);
			}

			return width;
		}

		readonly Dictionary<Graph<TNode, TTransition>.State, int> _ids = new Dictionary<Graph<TNode, TTransition>.State, int>();
	}
}
