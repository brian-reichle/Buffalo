// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Buffalo.Core.Common;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Parser.ParseItemSet, Buffalo.Core.Parser.Segment>;

namespace Buffalo.Core.Parser
{
	sealed class SVGParseGraphRenderer : SVGGraphRenderer<ParseItemSet, Segment>
	{
		public SVGParseGraphRenderer(Graph graph, IDictionary<Graph.State, int> stateMap)
			: base(graph)
		{
			_stateMap = stateMap;
			_stateText = new Dictionary<Graph.State, string[]>();
			_transitionText = new Dictionary<Segment, string>();
		}

		protected override void WriteResources(XmlWriter writer)
		{
			const string CSS = @"
rect
{
	fill: rgb(255,255,255);
	stroke-width: 1;
	stroke: rgb(0,0,0);
}

line, path
{
	stroke-width: 1;
	stroke: rgb(0,0,0);
	fill-opacity: 0.0;
}

text
{
	fill: rgb(0,0,0);
	font-family: sans-serif;
	font-size: 10px;
	text-anchor: left;
}

text.si
{
	font-size: 20px;
	text-anchor: end;
}
";

			base.WriteResources(writer);

			writer.WriteStartElement("style");
			writer.WriteAttributeString("type", "text/css");
			writer.WriteCData(CSS);
			writer.WriteEndElement();
		}

		protected override Size MeasureCore(Graph.State state)
		{
			var list = GetStateText(state);

			var maxLen = 0;

			for (var i = 0; i < list.Length; i++)
			{
				maxLen = Math.Max(maxLen, list[i].Length);
			}

			return new Size(10 + (maxLen * 5), 45 + (list.Length * 10));
		}

		protected override Size MeasureCore(Graph.Transition transition)
		{
			var text = GetTransitionText(transition);
			return new Size((text.Length * 5) + 10, 20);
		}

		protected override void RenderCore(Graph.State state, XmlWriter writer, Size size)
		{
			writer.WriteStartElement("rect");
			writer.WriteAttributeString("width", size.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("height", size.Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("rx", "20");
			writer.WriteEndElement();

			writer.WriteStartElement("line");
			writer.WriteAttributeString("x1", "0");
			writer.WriteAttributeString("y1", "25");
			writer.WriteAttributeString("x2", size.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("y2", "25");
			writer.WriteEndElement();

			writer.WriteStartElement("line");
			writer.WriteAttributeString("x1", "35");
			writer.WriteAttributeString("y1", "0");
			writer.WriteAttributeString("x2", "35");
			writer.WriteAttributeString("y2", "25");
			writer.WriteEndElement();

			writer.WriteStartElement("text");
			writer.WriteAttributeString("x", "30");
			writer.WriteAttributeString("y", "20");
			writer.WriteAttributeString("class", "si");
			writer.WriteString(_stateMap[state].ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();

			var y = 40;

			foreach (var line in GetStateText(state))
			{
				writer.WriteStartElement("text");
				writer.WriteAttributeString("x", "5");
				writer.WriteAttributeString("y", y.ToString(CultureInfo.InvariantCulture));
				writer.WriteString(line);
				writer.WriteEndElement();

				y += 10;
			}
		}

		protected override void RenderCore(Graph.Transition transition, XmlWriter writer, Size size)
		{
			writer.WriteStartElement("rect");
			writer.WriteAttributeString("width", size.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("height", size.Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();

			writer.WriteStartElement("text");
			writer.WriteAttributeString("x", "5");
			writer.WriteAttributeString("y", "15");
			writer.WriteString(GetTransitionText(transition));
			writer.WriteEndElement();
		}

		protected override void RenderCore(Graph.Transition transition, XmlWriter writer, Point[] path)
		{
			var builder = new StringBuilder();
			builder.Append('M');
			builder.Append(path[0].X);
			builder.Append(',');
			builder.Append(path[0].Y);

			for (var i = 3; i < path.Length; i += 3)
			{
				Point p;

				p = path[i - 2];
				builder.Append(" C");
				builder.Append(p.X);
				builder.Append(',');
				builder.Append(p.Y);

				p = path[i - 1];
				builder.Append(' ');
				builder.Append(p.X);
				builder.Append(',');
				builder.Append(p.Y);

				p = path[i];
				builder.Append(' ');
				builder.Append(p.X);
				builder.Append(',');
				builder.Append(p.Y);
			}

			writer.WriteStartElement("path");
			writer.WriteAttributeString("d", builder.ToString());
			writer.WriteEndElement();
		}

		string[] GetStateText(Graph.State state)
		{
			if (!_stateText.TryGetValue(state, out var result))
			{
				result = SplitLines(state.Label.ToString());
				_stateText.Add(state, result);
			}

			return result;
		}

		string GetTransitionText(Graph.Transition transition)
		{
			if (!_transitionText.TryGetValue(transition.Label, out var result))
			{
				result = transition.Label.ToString();
			}

			return result;
		}

		static string[] SplitLines(string text)
		{
			var start = 0;
			var result = new List<string>();

			for (var i = 0; i < text.Length; i++)
			{
				var c = text[i];

				if (c == '\r' || c == '\n')
				{
					if (i > start + 1)
					{
						result.Add(text.Substring(start, i - start));
					}

					start = i + 1;
				}
			}

			if (start < text.Length)
			{
				result.Add(text.Substring(start, text.Length - start));
			}

			var list = result.ToArray();
			return list;
		}

		readonly IDictionary<Graph.State, int> _stateMap;
		readonly Dictionary<Graph.State, string[]> _stateText;
		readonly Dictionary<Segment, string> _transitionText;
	}
}
