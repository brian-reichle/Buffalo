// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Buffalo.Core.Common;
using Buffalo.Core.Lexer.Configuration;
using Graph = Buffalo.Core.Common.Graph<Buffalo.Core.Lexer.NodeData, Buffalo.Core.Lexer.CharSet>;

namespace Buffalo.Core.Lexer
{
	sealed class CodeGen
	{
		public CodeGen(Config config)
		{
			_config = config;
			_stateData = new TableData[_config.Tables.Count];

			for (var i = 0; i < _stateData.Length; i++)
			{
				_stateData[i] = TableGenerator.ExtractData(_config, _config.Tables[i]);
			}

			_fixedStartState = FindFixedStartState();
		}

		public void Write(TextWriter writer)
		{
			writer.WriteLine("// <auto-generated />");
			writer.WriteLine("//------------------------------------------------------------------------------");
			writer.WriteLine("// This code is auto-generated.");
			writer.WriteLine("// Do not attempt to edit this file by hand, you could hurt yourself!");
			writer.WriteLine("//------------------------------------------------------------------------------");
			writer.WriteLine();
			writer.WriteLine("using System;");
			writer.WriteLine("using System.Collections;");
			writer.WriteLine("using System.Collections.Generic;");
			writer.WriteLine("using System.Diagnostics;");
			writer.WriteLine("using System.Threading;");
			writer.WriteLine();

			writer.Write("namespace ");
			writer.WriteLine(_config.Manager.ClassNamespace);

			writer.WriteLine("{");
			WriteScannerClass(writer);
			writer.WriteLine("}");
		}

		public void WriteTableResource(Stream stream, int tableID)
		{
			var blob = _stateData[tableID].TransitionTable;
			var size = blob.Bytes;
			var offset = 0;
			var buffer = new byte[Math.Min(size, 512)];

			while (offset < size)
			{
				var len = Math.Min(buffer.Length, size - offset);
				blob.CopyBytesTo(buffer, offset, len);
				stream.Write(buffer, 0, len);
				offset += len;
			}
		}

		void WriteScannerClass(TextWriter writer)
		{
			var size = ElementSizeStrategy.Get(_config.Manager.ElementSize);

			writer.Write("	");
			writer.Write(_config.Manager.Visibility == ClassVisibility.Public ? "public" : "internal");
			writer.Write(" abstract class ");
			writer.Write(_config.Manager.ClassName);
			writer.WriteLine("<TToken> : IEnumerable<TToken>, IEnumerator<TToken>");

			writer.WriteLine("		where TToken : class");
			writer.WriteLine("	{");
			WriteTokenType(writer);

			if (_config.States.Count > 1)
			{
				writer.WriteLine();
				WriteScannerState(writer);
			}

			writer.WriteLine();
			WriteStatistics(writer);
			WriteConstructor(writer);
			writer.WriteLine();
			WriteCopyConstructor(writer);
			writer.WriteLine();
			WriteDestructor(writer);
			writer.WriteLine();
			writer.WriteLine("		protected abstract TToken NewToken(TokenType type, string expressionString, int startPosition, int length);");
			writer.WriteLine();
			WriteNewScanner(writer);
			writer.WriteLine();
			WriteStartOfLine(writer);

			foreach (var method in ExtractDistinctCompressionMethods(size))
			{
				writer.WriteLine();
				method.Write(writer, 2);
			}

			if (_config.States.Count > 1)
			{
				writer.WriteLine();
				WriteStateManagementMethods(writer);
			}

			writer.WriteLine();
			WriteClassifyChar(writer);
			writer.WriteLine();
			WriteEnumerableMembers(writer);
			writer.WriteLine();
			WriteEnumeratorMembers(writer);
			writer.WriteLine();
			writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			writer.WriteLine("		TToken _currentToken;");
			writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			writer.WriteLine("		int _nextCharPosition;");
			writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			writer.WriteLine("		int _started;");

			if (!_fixedStartState.HasValue)
			{
				writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
				writer.WriteLine("		int _startState;");
			}

			if (_config.States.Count > 1)
			{
				writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
				writer.WriteLine("		ScannerState _currentState;");
				writer.WriteLine();
				writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
				writer.WriteLine("		readonly Stack<ScannerState> _state = new Stack<ScannerState>();");
			}
			else
			{
				writer.WriteLine();
			}

			writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			writer.WriteLine("		List<int> _solIndicies = new List<int>();");
			writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			writer.WriteLine("		readonly string _expressionString;");
			writer.WriteLine();
			WriteEnumeratorTableFields(writer);

			if (_config.Manager.CacheTables)
			{
				writer.WriteLine();
				writer.WriteLine("		[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
				writer.WriteLine("		readonly TableCache _cache;");
				writer.WriteLine();
				WriteTableCacheClass(writer);
			}

			writer.WriteLine("	}");
		}

		void WriteTableCacheClass(TextWriter writer)
		{
			writer.WriteLine("		class TableCache");
			writer.WriteLine("		{");
			writer.WriteLine("			protected TableCache()");
			writer.WriteLine("			{");
			WriteInitTableSets(writer, 4);
			writer.WriteLine("			}");
			writer.WriteLine();
			writer.WriteLine("			public static TableCache Get()");
			writer.WriteLine("			{");
			writer.WriteLine("				TableCache result;");
			writer.WriteLine();
			writer.WriteLine("				lock (_weakRef)");
			writer.WriteLine("				{");
			writer.WriteLine("					if ((result = (TableCache)_weakRef.Target) == null)");
			writer.WriteLine("					{");
			writer.WriteLine("						_weakRef.Target = result = new TableCache();");
			writer.WriteLine("					}");
			writer.WriteLine("				}");
			writer.WriteLine();
			writer.WriteLine("				return result;");
			writer.WriteLine("			}");
			writer.WriteLine();
			WriteCacheTableFields(writer);
			writer.WriteLine();
			writer.WriteLine("			[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			writer.WriteLine("			static readonly WeakReference _weakRef = new WeakReference(null);");
			writer.WriteLine("		}");
		}

		void WriteConstructor(TextWriter writer)
		{
			writer.Write("		protected ");
			writer.Write(_config.Manager.ClassName);
			writer.WriteLine("(string expressionString)");
			writer.WriteLine("		{");

			if (_config.Manager.CacheTables)
			{
				writer.WriteLine("			_cache = TableCache.Get();");

				if (_stateData.Length == 1)
				{
					writer.WriteLine("			_charClassificationBoundries = _cache._charClassificationBoundries;");
					writer.WriteLine("			_charClassification = _cache._charClassification;");
					writer.WriteLine("			_transitionTable = _cache._transitionTable;");
					writer.WriteLine("			_tokenTypes = _cache._tokenTypes;");
				}
			}
			else
			{
				WriteInitTableSets(writer, 3);
				writer.WriteLine();
			}

			writer.WriteLine("			_expressionString = expressionString;");

			if (_config.States.Count > 1)
			{
				writer.Write("			_currentState = ScannerState.");
				writer.Write(_config.States[0].Label.Text);
				writer.WriteLine(";");

				writer.Write("			SetForState(ScannerState.");
				writer.Write(_config.States[0].Label.Text);
				writer.WriteLine(");");
			}

			writer.WriteLine("		}");
		}

		void WriteCopyConstructor(TextWriter writer)
		{
			writer.Write("		protected ");
			writer.Write(_config.Manager.ClassName);
			writer.Write('(');
			writer.Write(_config.Manager.ClassName);
			writer.WriteLine("<TToken> source)");
			writer.WriteLine("		{");

			if (_config.Manager.CacheTables)
			{
				writer.WriteLine("			_cache = source._cache;");

				if (_stateData.Length == 1)
				{
					writer.WriteLine("			_charClassificationBoundries = source._charClassificationBoundries;");
					writer.WriteLine("			_charClassification = source._charClassification;");
					writer.WriteLine("			_transitionTable = source._transitionTable;");
					writer.WriteLine("			_tokenTypes = source._tokenTypes;");
				}
			}
			else
			{
				WriteCopyTableSets(writer);
				writer.WriteLine();
			}

			writer.WriteLine("			_expressionString = source._expressionString;");

			if (_config.States.Count > 1)
			{
				writer.Write("			_currentState = ScannerState.");
				writer.Write(_config.States[0].Label.Text);
				writer.WriteLine(";");

				writer.Write("			SetForState(ScannerState.");
				writer.Write(_config.States[0].Label.Text);
				writer.WriteLine(");");
			}

			writer.WriteLine("		}");
		}

		void WriteCopyTableSets(TextWriter writer)
		{
			if (_stateData.Length == 1)
			{
				WriteCopyTableSet(writer, null);
			}
			else if (_stateData.Length > 1)
			{
				WriteCopyTableSet(writer, _stateData[0].TableID.ToString(CultureInfo.InvariantCulture));

				for (var i = 1; i < _stateData.Length; i++)
				{
					writer.WriteLine();
					WriteCopyTableSet(writer, _stateData[i].TableID.ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		static void WriteCopyTableSet(TextWriter writer, string suffix)
		{
			WriteCopyTable(writer, "_charClassificationBoundries", suffix);
			WriteCopyTable(writer, "_charClassification", suffix);
			WriteCopyTable(writer, "_transitionTable", suffix);
			WriteCopyTable(writer, "_tokenTypes", suffix);
		}

		static void WriteCopyTable(TextWriter writer, string tablename, string suffix)
		{
			writer.Write("			");
			writer.Write(tablename);

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.Write(" = source.");
			writer.Write(tablename);

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.WriteLine(";");
		}

		void WriteInitTableSets(TextWriter writer, int indent)
		{
			if (_stateData.Length == 1)
			{
				WriteInitTableSet(writer, _stateData[0], _config.Tables[0], indent, null);
			}
			else if (_stateData.Length > 1)
			{
				WriteInitTableSet(writer, _stateData[0], _config.Tables[0], indent, _stateData[0].TableID.ToString(CultureInfo.InvariantCulture));

				for (var i = 1; i < _stateData.Length; i++)
				{
					writer.WriteLine();
					WriteInitTableSet(writer, _stateData[i], _config.Tables[i], indent, _stateData[i].TableID.ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		void WriteInitTableSet(TextWriter writer, TableData data, ConfigTable table, int indent, string suffix)
		{
			var size = ElementSizeStrategy.Get(_config.Manager.ElementSize);

			WriteLargeCharArray(writer, indent, "_charClassificationBoundries", suffix, 16, data.CharClassificationBoundries);
			WriteLargeIntArray(writer, indent, "_charClassification", suffix, 16, data.CharClassification, size);

			if (string.IsNullOrEmpty(table.Name))
			{
				WriteLargeIntArray(writer, indent, "_transitionTable", suffix, 16, data.TransitionTable);
			}
			else
			{
				WriteLargeIntArray(writer, indent, "_transitionTable", suffix, data.TransitionTable.Method, table.Name);
			}

			WriteTokenTable(writer, indent, table, suffix);
		}

		void WriteDestructor(TextWriter writer)
		{
			writer.Write("		~");
			writer.Write(_config.Manager.ClassName);
			writer.WriteLine("()");
			writer.WriteLine("		{");
			writer.WriteLine("			Dispose(false);");
			writer.WriteLine("		}");
		}

		void WriteStateManagementMethods(TextWriter writer)
		{
			writer.WriteLine("		protected ScannerState CurrentState");
			writer.WriteLine("		{");
			writer.WriteLine("			[DebuggerStepThrough]");
			writer.WriteLine("			get { return _currentState; }");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		protected void PushState(ScannerState state)");
			writer.WriteLine("		{");
			writer.WriteLine("			SetForState(state);");
			writer.WriteLine("			_state.Push(_currentState);");
			writer.WriteLine("			_currentState = state;");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		protected void PopState()");
			writer.WriteLine("		{");
			writer.WriteLine("			if (_state.Count > 0)");
			writer.WriteLine("			{");
			writer.WriteLine("				_currentState = _state.Pop();");
			writer.WriteLine("				SetForState(_currentState);");
			writer.WriteLine("			}");
			writer.WriteLine("			else");
			writer.WriteLine("			{");
			writer.WriteLine("				throw new InvalidOperationException(\"State stack is empty\");");
			writer.WriteLine("			}");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		void SetForState(ScannerState state)");
			writer.WriteLine("		{");
			writer.WriteLine("			switch (state)");
			writer.WriteLine("			{");

			var prefix = _config.Manager.CacheTables ? "_cache." : string.Empty;

			for (var i = 0; i < _config.States.Count; i++)
			{
				var entryPoint = _config.States[i];

				writer.Write("				case ScannerState.");
				writer.Write(entryPoint.Label.Text);
				writer.WriteLine(":");

				if (!_fixedStartState.HasValue)
				{
					var data = _stateData[entryPoint.GraphIndex];

					writer.Write("					_startState = ");
					writer.Write(data.StateMap[entryPoint.StartState]);
					writer.WriteLine(";");
				}

				if (_config.Tables.Count > 1)
				{
					writer.Write("					_transitionTable = ");
					writer.Write(prefix);
					writer.Write("_transitionTable_");
					writer.Write(entryPoint.GraphIndex);
					writer.WriteLine(";");

					writer.Write("					_tokenTypes = ");
					writer.Write(prefix);
					writer.Write("_tokenTypes_");
					writer.Write(entryPoint.GraphIndex);
					writer.WriteLine(";");

					writer.Write("					_charClassificationBoundries = ");
					writer.Write(prefix);
					writer.Write("_charClassificationBoundries_");
					writer.Write(entryPoint.GraphIndex);
					writer.WriteLine(";");

					writer.Write("					_charClassification = ");
					writer.Write(prefix);
					writer.Write("_charClassification_");
					writer.Write(entryPoint.GraphIndex);
					writer.WriteLine(";");
				}

				writer.WriteLine("					break;");
				writer.WriteLine();
			}

			writer.WriteLine("				default:");
			writer.WriteLine("					throw new InvalidOperationException(\"invalid state\");");
			writer.WriteLine("			}");
			writer.WriteLine("		}");
		}

		void WriteNewScanner(TextWriter writer)
		{
			writer.Write("		protected virtual ");
			writer.Write(_config.Manager.ClassName);
			writer.WriteLine("<TToken> NewScanner()");

			writer.WriteLine("		{");

			writer.Write("			");
			writer.Write(_config.Manager.ClassName);
			writer.Write("<TToken> result = (");
			writer.Write(_config.Manager.ClassName);
			writer.WriteLine("<TToken>)MemberwiseClone();");

			writer.WriteLine("			result._solIndicies = new List<int>();");
			writer.WriteLine("			result._currentToken = null;");
			writer.WriteLine("			result._nextCharPosition = 0;");
			writer.WriteLine("			return result;");
			writer.WriteLine("		}");
		}

		static void WriteStartOfLine(TextWriter writer)
		{
			writer.WriteLine("		protected void StartOfLine(int charIndex, out int lineNumber, out int charNumber)");
			writer.WriteLine("		{");
			writer.WriteLine("			int index = _solIndicies.BinarySearch(charIndex);");
			writer.WriteLine("			index = index < 0 ? -2 - index : index;");
			writer.WriteLine();
			writer.WriteLine("			lineNumber = index + 2;");
			writer.WriteLine("			charNumber = index < 0 ? charIndex + 1 : charIndex - _solIndicies[index] + 1;");
			writer.WriteLine("		}");
		}

		void WriteTokenType(TextWriter writer)
		{
			var size = ElementSizeStrategy.Get(_config.Manager.ElementSize);

			writer.Write("		public enum TokenType : ");
			writer.Write(size.Keyword);
			writer.WriteLine();
			writer.WriteLine("		{");
			writer.WriteLine("			EOF = 0,");

			var tokenTypes = _config.TokenTypes;

			for (var i = 0; i < tokenTypes.Count; i++)
			{
				writer.Write("			");
				writer.Write(tokenTypes[i]);
				writer.Write(" = ");
				writer.Write(i + 1);
				writer.WriteLine(",");
			}

			writer.WriteLine("		}");
		}

		void WriteScannerState(TextWriter writer)
		{
			writer.WriteLine("		public enum ScannerState");
			writer.WriteLine("		{");

			for (var i = 0; i < _config.States.Count; i++)
			{
				writer.Write("			");
				writer.Write(_config.States[i].Label.Text);
				writer.WriteLine(",");
			}

			writer.WriteLine("		}");
		}

		static void WriteClassifyChar(TextWriter writer)
		{
			writer.WriteLine("		int ClassifyChar(char c)");
			writer.WriteLine("		{");
			writer.WriteLine("			int lowerBound = 0;");
			writer.WriteLine("			int upperBound = _charClassificationBoundries.Length;");
			writer.WriteLine();
			writer.WriteLine("			while (lowerBound < upperBound)");
			writer.WriteLine("			{");
			writer.WriteLine("				int mid = (lowerBound + upperBound) >> 1;");
			writer.WriteLine();
			writer.WriteLine("				if (c <= _charClassificationBoundries[mid])");
			writer.WriteLine("				{");
			writer.WriteLine("					upperBound = mid;");
			writer.WriteLine("				}");
			writer.WriteLine("				else");
			writer.WriteLine("				{");
			writer.WriteLine("					lowerBound = mid + 1;");
			writer.WriteLine("				}");
			writer.WriteLine("			}");
			writer.WriteLine();
			writer.WriteLine("			return _charClassification[lowerBound];");
			writer.WriteLine("		}");
		}

		static void WriteEnumerableMembers(TextWriter writer)
		{
			writer.WriteLine("		#region IEnumerable<TToken> Members");
			writer.WriteLine();
			writer.WriteLine("		public IEnumerator<TToken> GetEnumerator()");
			writer.WriteLine("		{");
			writer.WriteLine("			if (Interlocked.Exchange(ref _started, 1) == 0)");
			writer.WriteLine("			{");
			writer.WriteLine("				return this;");
			writer.WriteLine("			}");
			writer.WriteLine("			else");
			writer.WriteLine("			{");
			writer.WriteLine("				return NewScanner();");
			writer.WriteLine("			}");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		#endregion");
			writer.WriteLine();
			writer.WriteLine("		#region IEnumerable Members");
			writer.WriteLine();
			writer.WriteLine("		IEnumerator IEnumerable.GetEnumerator()");
			writer.WriteLine("		{");
			writer.WriteLine("			return GetEnumerator();");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		#endregion");
		}

		void WriteEnumeratorMembers(TextWriter writer)
		{
			writer.WriteLine("		#region IEnumerator<TToken> Members");
			writer.WriteLine();
			writer.WriteLine("		TToken IEnumerator<TToken>.Current");
			writer.WriteLine("		{");
			writer.WriteLine("			[DebuggerStepThrough]");
			writer.WriteLine("			get { return _currentToken; }");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		#endregion");
			writer.WriteLine();
			writer.WriteLine("		#region IDisposable Members");
			writer.WriteLine();
			writer.WriteLine("		void IDisposable.Dispose()");
			writer.WriteLine("		{");
			writer.WriteLine("			Dispose(true);");
			writer.WriteLine("			GC.SuppressFinalize(this);");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		protected virtual void Dispose(bool isDisposing)");
			writer.WriteLine("		{");
			writer.WriteLine("		}");
			writer.WriteLine();
			writer.WriteLine("		#endregion");
			writer.WriteLine();
			writer.WriteLine("		#region IEnumerator Members");
			writer.WriteLine();
			writer.WriteLine("		object IEnumerator.Current");
			writer.WriteLine("		{");
			writer.WriteLine("			[DebuggerStepThrough]");
			writer.WriteLine("			get { return _currentToken; }");
			writer.WriteLine("		}");
			writer.WriteLine();
			WriteMoveNext(writer);
			writer.WriteLine();
			WriteReset(writer);
			writer.WriteLine();
			writer.WriteLine("		#endregion");
		}

		void WriteMoveNext(TextWriter writer)
		{
			writer.WriteLine("		bool IEnumerator.MoveNext()");
			writer.WriteLine("		{");
			writer.WriteLine("			do");
			writer.WriteLine("			{");
			writer.WriteLine("				if (_nextCharPosition > _expressionString.Length)");
			writer.WriteLine("				{");
			writer.WriteLine("					_currentToken = null;");
			writer.WriteLine("					return false;");
			writer.WriteLine("				}");
			writer.WriteLine("				else if (_nextCharPosition == _expressionString.Length)");
			writer.WriteLine("				{");
			writer.WriteLine("					_currentToken = NewToken(TokenType.EOF, _expressionString, _expressionString.Length, 0);");

			if (_config.States.Count > 1)
			{
				writer.WriteLine();
				writer.WriteLine("					if (_state.Count > 0)");
				writer.WriteLine("					{");
				writer.WriteLine("						PopState();");
				writer.WriteLine("					}");
				writer.WriteLine("					else");
				writer.WriteLine("					{");
				writer.WriteLine("						_nextCharPosition++;");
				writer.WriteLine("					}");
			}
			else
			{
				writer.WriteLine("					_nextCharPosition++;");
			}

			writer.WriteLine("				}");
			writer.WriteLine("				else");
			writer.WriteLine("				{");

			if (_fixedStartState.HasValue)
			{
				writer.Write("					int state = ");
				writer.Write(_fixedStartState.Value);
				writer.WriteLine(";");
			}
			else
			{
				writer.WriteLine("					int state = _startState;");
			}

			writer.WriteLine("					int startOfNextToken = -1;");
			writer.WriteLine("					TokenType type = TokenType.EOF;");
			writer.WriteLine("					int lastSolIndex = _solIndicies.Count > 0 ? _solIndicies[_solIndicies.Count - 1] : 0;");
			writer.WriteLine();
			writer.WriteLine("					for (int i = _nextCharPosition; i < _expressionString.Length; i++)");
			writer.WriteLine("					{");
			writer.WriteLine("						if (i > lastSolIndex)");
			writer.WriteLine("						{");
			writer.WriteLine("							if (_expressionString[i] == '\\n' ||");
			writer.WriteLine("								(_expressionString[i] == '\\r' && (i + 1 == _expressionString.Length || _expressionString[i + 1] != '\\n')))");
			writer.WriteLine("							{");
			writer.WriteLine("								_solIndicies.Add(lastSolIndex = i + 1);");
			writer.WriteLine("							}");
			writer.WriteLine("						}");
			writer.WriteLine();
			writer.WriteLine("						int offset = _transitionTable[state];");
			writer.WriteLine();
			writer.WriteLine("						if (offset == 0 || (state = _transitionTable[offset + ClassifyChar(_expressionString[i])] - 1) == -1)");
			writer.WriteLine("						{");
			writer.WriteLine("							break;");
			writer.WriteLine("						}");
			writer.WriteLine("						else");
			writer.WriteLine("						{");
			writer.WriteLine("							TokenType newType = _tokenTypes[state];");
			writer.WriteLine();
			writer.WriteLine("							if (newType != TokenType.EOF)");
			writer.WriteLine("							{");
			writer.WriteLine("								type = newType;");
			writer.WriteLine("								startOfNextToken = i + 1;");
			writer.WriteLine("							}");
			writer.WriteLine("						}");
			writer.WriteLine("					}");
			writer.WriteLine();
			writer.WriteLine("					if (type != TokenType.EOF)");
			writer.WriteLine("					{");
			writer.WriteLine("						// if end is less than the last value of i then you will end up re-reading some characters,");
			writer.WriteLine("						// if the start state is the only state that is not an end state then this will never happen.");
			writer.WriteLine("						_currentToken = NewToken(type, _expressionString, _nextCharPosition, startOfNextToken - _nextCharPosition);");
			writer.WriteLine("						_nextCharPosition = startOfNextToken;");
			writer.WriteLine("					}");
			writer.WriteLine("					else");
			writer.WriteLine("					{");
			writer.WriteLine("						// if you write your rules properly then this should never happen.");
			writer.WriteLine("						// eg. add a rule at the end that matches a single instance of any character.");
			writer.WriteLine("						throw new InvalidOperationException(string.Format(\"Got stuck at position {0}.\", _nextCharPosition));");
			writer.WriteLine("					}");
			writer.WriteLine("				}");
			writer.WriteLine("			}");
			writer.WriteLine("			while (_currentToken == null);");
			writer.WriteLine();
			writer.WriteLine("			return true;");
			writer.WriteLine("		}");
		}

		void WriteReset(TextWriter writer)
		{
			writer.WriteLine("		void IEnumerator.Reset()");
			writer.WriteLine("		{");
			writer.WriteLine("			_currentToken = null;");
			writer.WriteLine("			_nextCharPosition = 0;");
			writer.WriteLine("			_solIndicies.Clear();");

			if (_config.States.Count > 1)
			{
				writer.WriteLine("			_state.Clear();");

				writer.Write("			_currentState = ScannerState.");
				writer.Write(_config.States[0].Label.Text);
				writer.WriteLine(";");

				writer.Write("			SetForState(ScannerState.");
				writer.Write(_config.States[0].Label.Text);
				writer.WriteLine(");");
			}

			writer.WriteLine("		}");
		}

		void WriteEnumeratorTableFields(TextWriter writer)
		{
			var size = ElementSizeStrategy.Get(_config.Manager.ElementSize);

			if (_config.Tables.Count > 1)
			{
				WriteTableFields(writer, 2, null, size, false, false);

				if (!_config.Manager.CacheTables)
				{
					writer.WriteLine();

					var table = _config.Tables[0];
					WriteTableFields(writer, 2, table.Index.ToString(CultureInfo.InvariantCulture), size, true, false);

					for (var i = 1; i < _config.Tables.Count; i++)
					{
						table = _config.Tables[i];

						writer.WriteLine();
						WriteTableFields(writer, 2, table.Index.ToString(CultureInfo.InvariantCulture), size, true, false);
					}
				}
			}
			else
			{
				WriteTableFields(writer, 2, null, size, true, false);
			}
		}

		void WriteCacheTableFields(TextWriter writer)
		{
			var size = ElementSizeStrategy.Get(_config.Manager.ElementSize);

			if (_config.Tables.Count > 1)
			{
				var table = _config.Tables[0];
				WriteTableFields(writer, 3, table.Index.ToString(CultureInfo.InvariantCulture), size, true, true);

				for (var i = 1; i < _config.Tables.Count; i++)
				{
					table = _config.Tables[i];

					writer.WriteLine();
					WriteTableFields(writer, 3, table.Index.ToString(CultureInfo.InvariantCulture), size, true, true);
				}
			}
			else
			{
				WriteTableFields(writer, 3, null, size, true, true);
			}
		}

		static void WriteTableFields(TextWriter writer, int indent, string suffix, ElementSizeStrategy elementSize, bool readOnly, bool @public)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			CodeGenHelper.WriteIndent(writer, indent);

			if (@public)
			{
				writer.Write("public ");
			}

			if (readOnly)
			{
				writer.Write("readonly ");
			}

			writer.Write("char[] _charClassificationBoundries");

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.WriteLine(";");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			CodeGenHelper.WriteIndent(writer, indent);

			if (@public)
			{
				writer.Write("public ");
			}

			if (readOnly)
			{
				writer.Write("readonly ");
			}

			writer.Write(elementSize.Keyword);
			writer.Write("[] _charClassification");

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.WriteLine(";");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			CodeGenHelper.WriteIndent(writer, indent);

			if (@public)
			{
				writer.Write("public ");
			}

			if (readOnly)
			{
				writer.Write("readonly ");
			}

			writer.Write(elementSize.Keyword);
			writer.Write("[] _transitionTable");

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.WriteLine(";");
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
			CodeGenHelper.WriteIndent(writer, indent);

			if (@public)
			{
				writer.Write("public ");
			}

			if (readOnly)
			{
				writer.Write("readonly ");
			}

			writer.Write("TokenType[] _tokenTypes");

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.WriteLine(";");
		}

		void WriteTokenTable(TextWriter writer, int indent, ConfigTable table, string suffix)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write("_tokenTypes");

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.Write(" = new TokenType[");
			writer.Write(table.Graph.States.Count);
			writer.WriteLine(']');
			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine('{');

			var data = _stateData[table.Index];
			var stateLabels = GetStateLabels(table.Graph, data);
			var typeLabels = new string[stateLabels.Length];

			foreach (var state in table.Graph.States)
			{
				var endid = state.Label.EndState;
				typeLabels[data.StateMap[state]] = endid.HasValue ? _config.TokenTypes[endid.Value] : "EOF";
			}

			for (var i = 0; i < typeLabels.Length; i++)
			{
				CodeGenHelper.WriteIndent(writer, indent + 1);
				writer.Write("TokenType.");
				writer.Write(typeLabels[i]);

				var label = stateLabels[i];

				if (!string.IsNullOrEmpty(label))
				{
					writer.Write(", // ");
					writer.Write(label);
				}
				else
				{
					writer.Write(',');
				}

				writer.WriteLine();
			}

			CodeGenHelper.WriteIndent(writer, indent);
			writer.WriteLine("};");
		}

		void WriteStatistics(TextWriter writer)
		{
			var size = ElementSizeStrategy.Get(_config.Manager.ElementSize);

			if (_stateData.Length == 1)
			{
				var data = _stateData[0];
				WriteStatistics(writer, data.Statistics, null, size);
			}
			else if (_stateData.Length > 1)
			{
				WriteStatistics(writer, _stateData[0].Statistics, _stateData[0].TableID.ToString(CultureInfo.InvariantCulture), size);

				for (var i = 1; i < _stateData.Length; i++)
				{
					writer.WriteLine();
					WriteStatistics(writer, _stateData[i].Statistics, _stateData[i].TableID.ToString(CultureInfo.InvariantCulture), size);
				}
			}
		}

		static void WriteStatistics(TextWriter writer, Statistics statistics, string suffix, ElementSizeStrategy elementSize)
		{
			var transitionsNatural = statistics.States * statistics.CharClassifications;
			writer.Write("		// [Statistics");

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write(" Table ");
				writer.Write(suffix);
			}

			writer.WriteLine("]");

			writer.Write("		// Char Classifications : ");
			writer.WriteLine(statistics.CharClassifications);
			writer.Write("		// Char Ranges          : ");
			writer.WriteLine(statistics.CharRanges);
			writer.Write("		// States               : ");
			writer.WriteLine(statistics.States);
			writer.Write("		//   Terminal           : ");
			writer.WriteLine(statistics.StatesTerminal);
			writer.Write("		// Transition Table     : ");
			writer.Write(statistics.TransitionsRunTime);
			writer.Write("/");
			writer.Write(transitionsNatural);
			writer.Write(" (");
			writer.Write(((statistics.TransitionsRunTime / (decimal)transitionsNatural) * 100).ToString("0.00", CultureInfo.InvariantCulture));
			writer.WriteLine("%)");
			writer.Write("		//   Offsets            : ");
			writer.WriteLine(statistics.States);
			writer.Write("		//   Actions            : ");
			writer.WriteLine(statistics.TransitionsRunTime - statistics.States);

			var boundryBytes = statistics.CharRanges << 1;
			var classificationBytes = elementSize.Size(statistics.CharRanges);
			var transitionBytes = elementSize.Size(statistics.TransitionsRunTime);
			var tokenTypeBytes = elementSize.Size(statistics.States);
			var memoryFootprint = boundryBytes + classificationBytes + transitionBytes + tokenTypeBytes;
			var assemblyFootprint = boundryBytes + classificationBytes + statistics.TransitionsAssemblyBytes + tokenTypeBytes;

			writer.Write("		// Memory Footprint     : ");
			writer.Write(memoryFootprint);
			writer.WriteLine(" bytes");
			writer.Write("		//   Boundries          : ");
			writer.Write(boundryBytes);
			writer.WriteLine(" bytes");
			writer.Write("		//   Classifications    : ");
			writer.Write(classificationBytes);
			writer.WriteLine(" bytes");
			writer.Write("		//   Transitions        : ");
			writer.Write(transitionBytes);
			writer.WriteLine(" bytes");
			writer.Write("		//   Token Types        : ");
			writer.Write(tokenTypeBytes);
			writer.WriteLine(" bytes");
			writer.Write("		// Assembly Footprint   : ");
			writer.Write(assemblyFootprint);
			writer.Write(" bytes (");
			writer.Write(((assemblyFootprint / (decimal)memoryFootprint) * 100).ToString("0.00", CultureInfo.InvariantCulture));
			writer.WriteLine("%)");
		}

		static void WriteLargeIntArray(TextWriter writer, int indent, string name, string suffix, int wrap, CompressedBlob data)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(name);

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.Write(" = ");
			CodeGenHelper.WriteLargeIntArray(writer, indent, wrap, data);
			writer.WriteLine(";");
		}

		static void WriteLargeIntArray(TextWriter writer, int indent, string name, string suffix, int wrap, IList<int> data, ElementSizeStrategy elementSize)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(name);

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.Write(" = ");
			CodeGenHelper.WriteLargeIntArray(writer, indent, wrap, data, elementSize);
			writer.WriteLine(";");
		}

		static void WriteLargeIntArray(TextWriter writer, int indent, string name, string suffix, Compression method, string tableResourseName)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(name);

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.Write(" = ");
			CodeGenHelper.WriteLargeIntArray(writer, method, tableResourseName);
			writer.WriteLine(";");
		}

		static void WriteLargeCharArray(TextWriter writer, int indent, string name, string suffix, int wrap, IList<char> data)
		{
			CodeGenHelper.WriteIndent(writer, indent);
			writer.Write(name);

			if (!string.IsNullOrEmpty(suffix))
			{
				writer.Write('_');
				writer.Write(suffix);
			}

			writer.Write(" = ");
			CodeGenHelper.WriteLargeCharArray(writer, indent, wrap, data);
			writer.WriteLine(";");
		}

		IEnumerable<HelperMethod> ExtractDistinctCompressionMethods(ElementSizeStrategy sizeStrategy)
		{
			var result = new List<HelperMethod>();

			foreach (var data in _stateData)
			{
				var resourceName = _config.Tables[data.TableID].Name;

				foreach (var method in HelperMethod.GetDecompressionMethods(data.TransitionTable.Method, sizeStrategy, !string.IsNullOrEmpty(resourceName)))
				{
					if (!result.Contains(method))
					{
						result.Add(method);
					}
				}
			}

			return result;
		}

		static string[] GetStateLabels(Graph graph, TableData data)
		{
			var pending = new Queue<Graph.State>();
			var assemble = new string[graph.States.Count];

			foreach (var state in graph.StartStates)
			{
				assemble[data.StateMap[state]] = string.Empty;
				pending.Enqueue(state);
			}

			while (pending.Count > 0)
			{
				var fromState = pending.Dequeue();
				var prefix = assemble[data.StateMap[fromState]];

				foreach (var transition in fromState.ToTransitions)
				{
					var set = transition.Label;
					var toState = transition.ToState;

					if (toState == null) continue;

					var index = data.StateMap[toState];

					if (assemble[index] != null) continue;

					var builder = new StringBuilder(prefix);
					set.AppendTo(builder);
					assemble[index] = builder.ToString();

					pending.Enqueue(toState);
				}
			}

			return assemble;
		}

		int? FindFixedStartState()
		{
			using (IEnumerator<ConfigState> enumerator = _config.States.GetEnumerator())
			{
				if (!enumerator.MoveNext()) return null;

				var result = GetMappedStartState(enumerator.Current);

				while (enumerator.MoveNext())
				{
					var next = GetMappedStartState(enumerator.Current);

					if (next != result) return null;
				}

				return result;
			}
		}

		int GetMappedStartState(ConfigState entryPoint)
		{
			var data = _stateData[entryPoint.GraphIndex];
			return data.StateMap[entryPoint.StartState];
		}

		readonly Config _config;
		readonly int? _fixedStartState;
		readonly TableData[] _stateData;
	}
}
