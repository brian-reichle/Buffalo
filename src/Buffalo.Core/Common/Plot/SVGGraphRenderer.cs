// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Buffalo.Core.Common
{
	abstract class SVGGraphRenderer<TNode, TTransition> : IAdditionalFile
	{
		public SVGGraphRenderer(Graph<TNode, TTransition> graph)
		{
			_graph = graph;
			_stateSizes = new Dictionary<Graph<TNode, TTransition>.State, Size>();
			_transitionSizes = new Dictionary<Graph<TNode, TTransition>.Transition, Size>();
			_ids = new Dictionary<Graph<TNode, TTransition>.State, int>();
		}

		public void GenerateFileContent(TextWriter writer)
		{
			var info = new Info();
			GraphLayoutEngine<TNode, TTransition>.Calculate(new Spec(this), info, _graph);

			var settings = new XmlWriterSettings()
			{
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Document,
				Indent = true,
				IndentChars = "\t",
			};

			using (var xmlWriter = XmlWriter.Create(writer, settings))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement(string.Empty, "svg", "http://www.w3.org/2000/svg");
				xmlWriter.WriteAttributeString("version", "1.1");
				xmlWriter.WriteAttributeString("width", (info.MaxX - info.MinX + 1).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("height", (info.MaxY - info.MinY + 1).ToString(CultureInfo.InvariantCulture));

				WriteResources(xmlWriter);

				var originShift = info.MinX != 0 || info.MinY != 0;

				if (originShift)
				{
					xmlWriter.WriteStartElement("g");
					WriteTranslationAttribute(xmlWriter, -info.MinX, -info.MinY);
				}

				Render(xmlWriter, info);

				if (originShift)
				{
					xmlWriter.WriteEndElement();
				}

				xmlWriter.WriteEndElement();

				xmlWriter.WriteEndDocument();
				xmlWriter.Flush();
			}
		}

		protected abstract Size MeasureCore(Graph<TNode, TTransition>.State state);
		protected abstract Size MeasureCore(Graph<TNode, TTransition>.Transition transition);

		protected abstract void RenderCore(Graph<TNode, TTransition>.State state, XmlWriter writer, Size size);
		protected abstract void RenderCore(Graph<TNode, TTransition>.Transition transition, XmlWriter writer, Size size);
		protected abstract void RenderCore(Graph<TNode, TTransition>.Transition transition, XmlWriter writer, Point[] path);

		protected virtual void WriteResources(XmlWriter writer)
		{
		}

		protected virtual string Suffix => ".svg";

		protected int GetId(Graph<TNode, TTransition>.State state)
		{
			if (!_ids.TryGetValue(state, out var result))
			{
				_ids.Add(state, result = _ids.Count + 1);
			}

			return result;
		}

		void Render(XmlWriter writer, Info info)
		{
			foreach (var path in info.Paths)
			{
				Render(writer, path);
			}

			foreach (var node in info.States)
			{
				Render(writer, node);
			}

			foreach (var node in info.Transitions)
			{
				Render(writer, node);
			}
		}

		void Render(XmlWriter writer, NodeLayout<Graph<TNode, TTransition>.State> node)
		{
			writer.WriteStartElement("g");
			WriteTranslationAttribute(writer, node.Location.X, node.Location.Y);
			RenderCore(node.Data, writer, node.Size);
			writer.WriteEndElement();
		}

		void Render(XmlWriter writer, NodeLayout<Graph<TNode, TTransition>.Transition> node)
		{
			writer.WriteStartElement("g");
			WriteTranslationAttribute(writer, node.Location.X, node.Location.Y);
			RenderCore(node.Data, writer, node.Size);
			writer.WriteEndElement();
		}

		void Render(XmlWriter writer, PathLayout path)
		{
			RenderCore(path.Transition, writer, path.Path);
		}

		static void WriteTranslationAttribute(XmlWriter writer, int x, int y)
		{
			writer.WriteAttributeString("transform", string.Format(CultureInfo.InvariantCulture, "translate({0},{1})", x, y));
		}

		Size Measure(Graph<TNode, TTransition>.State state)
		{
			if (!_stateSizes.TryGetValue(state, out var result))
			{
				result = MeasureCore(state);
				_stateSizes.Add(state, result);
			}

			return result;
		}

		Size Measure(Graph<TNode, TTransition>.Transition transition)
		{
			if (!_transitionSizes.TryGetValue(transition, out var result))
			{
				result = MeasureCore(transition);
				_transitionSizes.Add(transition, result);
			}

			return result;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string IAdditionalFile.Suffix => Suffix;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		AdditionalFileType IAdditionalFile.Type => AdditionalFileType.None;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IAdditionalFile.IsBinary => false;
		void IAdditionalFile.GenerateFileContent(Stream stream) => throw new NotSupportedException();

		[DebuggerDisplay("Size ({Width, Height})")]
		protected struct Size
		{
			public Size(int width, int height)
			{
				Width = width;
				Height = height;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Width { get; }
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Height { get; }
		}

		[DebuggerDisplay("Point ({X}, {Y})]")]
		protected struct Point
		{
			public Point(int x, int y)
			{
				X = x;
				Y = y;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int X { get; }
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Y { get; }
		}

		struct NodeLayout<T>
		{
			public NodeLayout(T data, Point location, Size size)
			{
				Data = data;
				Location = location;
				Size = size;
			}

			public T Data;
			public readonly Point Location;
			public readonly Size Size;
		}

		struct PathLayout
		{
			public PathLayout(Graph<TNode, TTransition>.Transition transition, Point[] path)
			{
				Transition = transition;
				Path = path;
			}

			public readonly Graph<TNode, TTransition>.Transition Transition;
			public readonly Point[] Path;
		}

		sealed class Spec : IGraphLayoutSpec<TNode, TTransition>
		{
			public Spec(SVGGraphRenderer<TNode, TTransition> host)
			{
				_host = host;
			}

			public int NodeSeparation => 25;
			public int RankSeparation => 50;

			public int NodeWidth(Graph<TNode, TTransition>.State state) => _host.Measure(state).Width;
			public int NodeHeight(Graph<TNode, TTransition>.State state) => _host.Measure(state).Height;
			public int LabelWidth(Graph<TNode, TTransition>.Transition transition) => _host.Measure(transition).Width;
			public int LabelHeight(Graph<TNode, TTransition>.Transition transition) => _host.Measure(transition).Height;

			readonly SVGGraphRenderer<TNode, TTransition> _host;
		}

		sealed class Info : IGraphLayoutInfo<TNode, TTransition>
		{
			public Info()
			{
				States = new List<NodeLayout<Graph<TNode, TTransition>.State>>();
				Transitions = new List<NodeLayout<Graph<TNode, TTransition>.Transition>>();
				Paths = new List<PathLayout>();

				MinX = MinY = int.MaxValue;
				MaxX = MaxY = int.MinValue;
			}

			public void SetStatePosition(Graph<TNode, TTransition>.State state, int x, int y, int width, int height)
			{
				UpdateBounds(x, y, width, height);

				States.Add(new NodeLayout<Graph<TNode, TTransition>.State>(
					state,
					new Point(x, y),
					new Size(width, height)));
			}

			public void SetTransitionLabel(Graph<TNode, TTransition>.Transition transition, int x, int y, int width, int height)
			{
				UpdateBounds(x, y, width, height);

				Transitions.Add(new NodeLayout<Graph<TNode, TTransition>.Transition>(
					transition,
					new Point(x, y),
					new Size(width, height)));
			}

			public void SetTransitionPath(Graph<TNode, TTransition>.Transition transition, int[] x, int[] y)
			{
				var path = new Point[x.Length];

				for (var i = 0; i < path.Length; i++)
				{
					UpdateBounds(x[i], y[i]);
					path[i] = new Point(x[i], y[i]);
				}

				Paths.Add(new PathLayout(transition, path));
			}

			public int MinX { get; private set; }
			public int MinY { get; private set; }
			public int MaxX { get; private set; }
			public int MaxY { get; private set; }

			void UpdateBounds(int x, int y)
			{
				if (x < MinX)
				{
					MinX = x;
				}

				if (x > MaxX)
				{
					MaxX = x;
				}

				if (y < MinY)
				{
					MinY = y;
				}

				if (y > MaxY)
				{
					MaxY = y;
				}
			}

			void UpdateBounds(int x, int y, int width, int height)
			{
				UpdateBounds(x, y);
				UpdateBounds(x + width, y + height);
			}

			public List<NodeLayout<Graph<TNode, TTransition>.State>> States { get; }
			public List<NodeLayout<Graph<TNode, TTransition>.Transition>> Transitions { get; }
			public List<PathLayout> Paths { get; }
		}

		readonly Graph<TNode, TTransition> _graph;
		readonly Dictionary<Graph<TNode, TTransition>.State, Size> _stateSizes;
		readonly Dictionary<Graph<TNode, TTransition>.Transition, Size> _transitionSizes;
		readonly Dictionary<Graph<TNode, TTransition>.State, int> _ids;
	}
}
