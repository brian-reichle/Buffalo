// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Globalization;
using System.IO;
using Buffalo.Core.Parser.Configuration;

namespace Buffalo.Core.Parser
{
	sealed class ParseTableGen : IAdditionalFile
	{
		public ParseTableGen(Config config, TableData data)
		{
			_config = config;
			_data = data;
		}

		public void GenerateFileContent(TextWriter writer)
			=> WriteCommentTable(writer, GenerateTextualParseTable());

		string[][] GenerateTextualParseTable()
		{
			var width = 0;

			foreach (var pair in _data.GotoMap)
			{
				if (width < pair.Value)
				{
					width = pair.Value;
				}
			}

			width += _data.TerminalColumns;

			var table = new string[_data.StateMap.Count + 1][];
			var row0 = new string[width + 2];
			table[0] = row0;

			foreach (var pair in _data.TerminalMap)
			{
				var x = (pair.Value & _data.TerminalMask) + 1;
				var existing = row0[x];

				if (existing == null)
				{
					row0[x] = pair.Key.Name;
				}
				else
				{
					row0[x] = existing + '/' + pair.Key.Name;
				}
			}

			foreach (var pair in _data.GotoMap)
			{
				var x = pair.Value + _data.TerminalColumns + 1;
				var existing = row0[x];

				if (existing == null)
				{
					row0[x] = pair.Key.Name;
				}
				else
				{
					row0[x] = existing + '/' + pair.Key.Name;
				}
			}

			foreach (var state in _config.Graph.Graph.States)
			{
				var id = _data.StateMap[state];
				var row = new string[width + 2];

				table[id + 1] = row;
				row[0] = id.ToString(CultureInfo.InvariantCulture);

				foreach (var item in state.Label)
				{
					if (item.Position != item.Production.Segments.Length) continue;

					string text;

					if (item.Production.Target.IsInitial)
					{
						text = "accept";
					}
					else
					{
						text = item.Production.ToString();
					}

					foreach (var la in state.Label.GetLookahead(item))
					{
						var index = _data.TerminalMap[la];

						if ((index & ~_data.TerminalMask) == 0)
						{
							row[index + 1] = text;
						}
					}
				}

				foreach (var transition in state.ToTransitions)
				{
					if (transition.Label.IsTerminal)
					{
						var index = _data.TerminalMap[transition.Label];

						if ((index & ~_data.TerminalMask) == 0)
						{
							row[index + 1] = string.Format(CultureInfo.InvariantCulture, "S{0}", _data.StateMap[transition.ToState]);
						}
					}
					else
					{
						var index = _data.GotoMap[transition.Label] + _data.TerminalColumns + 1;
						var s = row[index];

						if (s == null)
						{
							row[index] = string.Format(CultureInfo.InvariantCulture, "{0} ({1}", _data.StateMap[transition.ToState], transition.Label.Name);
						}
						else
						{
							row[index] = s + "/" + transition.Label.Name;
						}
					}
				}

				for (var i = _data.TerminalColumns + 1; i < row.Length; i++)
				{
					var s = row[i];

					if (s != null)
					{
						row[i] = s + ")";
					}
				}
			}

			return table;
		}

		static void WriteCommentTable(TextWriter writer, string[][] table)
		{
			var colWidths = CalculateWidths(table);

			writer.Write('+');

			for (var w = 0; w < colWidths.Length; w++)
			{
				writer.Write(new string('-', colWidths[w] + 2));
				writer.Write('+');
			}

			writer.WriteLine();

			for (var r = 0; r < table.Length; r++)
			{
				var row = table[r];

				writer.Write('|');

				for (var c = 0; c < colWidths.Length; c++)
				{
					var val = row[c];
					writer.Write(' ');
					writer.Write(val);
					writer.Write(new string(' ', colWidths[c] - (val == null ? 0 : val.Length)));
					writer.Write(" |");
				}

				writer.WriteLine();
				writer.Write('+');

				for (var c = 0; c < colWidths.Length; c++)
				{
					writer.Write(new string('-', colWidths[c] + 2));
					writer.Write('+');
				}

				writer.WriteLine();
			}
		}

		static int[] CalculateWidths(string[][] table)
		{
			var colWidths = new int[table[0].Length];

			for (var i = 0; i < table.Length; i++)
			{
				var row = table[i];

				for (var j = 0; j < row.Length; j++)
				{
					var val = row[j];
					if (val != null && val.Length > colWidths[j])
					{
						colWidths[j] = val.Length;
					}
				}
			}

			return colWidths;
		}

		string IAdditionalFile.Suffix => ".txt";
		bool IAdditionalFile.IsBinary => false;
		AdditionalFileType IAdditionalFile.Type => AdditionalFileType.None;
		void IAdditionalFile.GenerateFileContent(Stream stream) => throw new NotSupportedException();

		readonly Config _config;
		readonly TableData _data;
	}
}
