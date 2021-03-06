// <auto-generated />
//------------------------------------------------------------------------------
// This code is auto-generated.
// Do not attempt to edit this file by hand, you could hurt yourself!
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Buffalo.Core.Parser.Configuration
{
	internal abstract class AutoConfigScanner<TToken> : IEnumerable<TToken>, IEnumerator<TToken>
		where TToken : class
	{
		public enum TokenType : ushort
		{
			EOF = 0,
			Assign = 1,
			QuestionMark = 2,
			Bang = 3,
			Semicolon = 4,
			Pipe = 5,
			Becomes = 6,
			OpenBrace = 7,
			CloseBrace = 8,
			OpenParen = 9,
			CloseParen = 10,
			Null = 11,
			Using = 12,
			Entry = 13,
			TargetValue = 14,
			ArgumentValue = 15,
			Label = 16,
			String = 17,
			BrokenString = 18,
			NonTerminal = 19,
			BrokenNonTerminal = 20,
			Comment = 21,
			BrokenComment = 22,
			Whitespace = 23,
			Error = 24,
		}

		// [Statistics]
		// Char Classifications : 32
		// Char Ranges          : 51
		// States               : 44
		//   Terminal           : 15
		// Transition Table     : 650/1408 (46.16%)
		//   Offsets            : 44
		//   Actions            : 606
		// Memory Footprint     : 1592 bytes
		//   Boundries          : 102 bytes
		//   Classifications    : 102 bytes
		//   Transitions        : 1300 bytes
		//   Token Types        : 88 bytes
		// Assembly Footprint   : 521 bytes (32.73%)
		protected AutoConfigScanner(string expressionString)
		{
			_cache = TableCache.Get();
			_charClassificationBoundries = _cache._charClassificationBoundries;
			_charClassification = _cache._charClassification;
			_transitionTable = _cache._transitionTable;
			_tokenTypes = _cache._tokenTypes;
			_expressionString = expressionString;
		}

		protected AutoConfigScanner(AutoConfigScanner<TToken> source)
		{
			_cache = source._cache;
			_charClassificationBoundries = source._charClassificationBoundries;
			_charClassification = source._charClassification;
			_transitionTable = source._transitionTable;
			_tokenTypes = source._tokenTypes;
			_expressionString = source._expressionString;
		}

		~AutoConfigScanner()
		{
			Dispose(false);
		}

		protected abstract TToken NewToken(TokenType type, string expressionString, int startPosition, int length);

		protected virtual AutoConfigScanner<TToken> NewScanner()
		{
			AutoConfigScanner<TToken> result = (AutoConfigScanner<TToken>)MemberwiseClone();
			result._solIndicies = new List<int>();
			result._currentToken = null;
			result._nextCharPosition = 0;
			return result;
		}

		protected void StartOfLine(int charIndex, out int lineNumber, out int charNumber)
		{
			int index = _solIndicies.BinarySearch(charIndex);
			index = index < 0 ? -2 - index : index;

			lineNumber = index + 2;
			charNumber = index < 0 ? charIndex + 1 : charIndex - _solIndicies[index] + 1;
		}

		static ushort[] Expand(string resourceName)
		{
			using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				const byte FOLLOW = 0x80;
				const byte REPEAT = 0x40;
				const byte FIRSTBODY = 0x3F;
				const byte SUBBODY = 0x7F;

				int value;
				byte tmp;

				tmp = unchecked((byte)stream.ReadByte());
				value = tmp & FIRSTBODY;

				while ((tmp & FOLLOW) != 0)
				{
					tmp = unchecked((byte)stream.ReadByte());
					value = (value << 7) | (tmp & SUBBODY);
				}

				int write = 0;
				ushort[] result = new ushort[value];

				while (stream.Position < stream.Length)
				{
					tmp = unchecked((byte)stream.ReadByte());
					if ((tmp & REPEAT) == 0)
					{
						value = tmp & FIRSTBODY;

						while ((tmp & FOLLOW) != 0)
						{
							tmp = unchecked((byte)stream.ReadByte());
							value = (value << 7) | (tmp & SUBBODY);
						}

						result[write++] = unchecked((ushort)value);
					}
					else
					{
						int count = tmp & FIRSTBODY;

						while ((tmp & FOLLOW) != 0)
						{
							tmp = unchecked((byte)stream.ReadByte());
							count = (count << 7) | (tmp & SUBBODY);
						}

						tmp = unchecked((byte)stream.ReadByte());
						value = tmp & FIRSTBODY;

						while ((tmp & FOLLOW) != 0)
						{
							tmp = unchecked((byte)stream.ReadByte());
							value = (value << 7) | (tmp & SUBBODY);
						}

						while (count > 0)
						{
							result[write++] = unchecked((ushort)value);
							count--;
						}
					}
				}

				return result;
			}
		}

		int ClassifyChar(char c)
		{
			int lowerBound = 0;
			int upperBound = _charClassificationBoundries.Length;

			while (lowerBound < upperBound)
			{
				int mid = (lowerBound + upperBound) >> 1;

				if (c <= _charClassificationBoundries[mid])
				{
					upperBound = mid;
				}
				else
				{
					lowerBound = mid + 1;
				}
			}

			return _charClassification[lowerBound];
		}

		#region IEnumerable<TToken> Members

		public IEnumerator<TToken> GetEnumerator()
		{
			if (Interlocked.Exchange(ref _started, 1) == 0)
			{
				return this;
			}
			else
			{
				return NewScanner();
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IEnumerator<TToken> Members

		TToken IEnumerator<TToken>.Current
		{
			[DebuggerStepThrough]
			get { return _currentToken; }
		}

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
		}

		#endregion

		#region IEnumerator Members

		object IEnumerator.Current
		{
			[DebuggerStepThrough]
			get { return _currentToken; }
		}

		bool IEnumerator.MoveNext()
		{
			do
			{
				if (_nextCharPosition > _expressionString.Length)
				{
					_currentToken = null;
					return false;
				}
				else if (_nextCharPosition == _expressionString.Length)
				{
					_currentToken = NewToken(TokenType.EOF, _expressionString, _expressionString.Length, 0);
					_nextCharPosition++;
				}
				else
				{
					int state = 0;
					int startOfNextToken = -1;
					TokenType type = TokenType.EOF;
					int lastSolIndex = _solIndicies.Count > 0 ? _solIndicies[_solIndicies.Count - 1] : 0;

					for (int i = _nextCharPosition; i < _expressionString.Length; i++)
					{
						if (i > lastSolIndex)
						{
							if (_expressionString[i] == '\n' ||
								(_expressionString[i] == '\r' && (i + 1 == _expressionString.Length || _expressionString[i + 1] != '\n')))
							{
								_solIndicies.Add(lastSolIndex = i + 1);
							}
						}

						int offset = _transitionTable[state];

						if (offset == 0 || (state = _transitionTable[offset + ClassifyChar(_expressionString[i])] - 1) == -1)
						{
							break;
						}
						else
						{
							TokenType newType = _tokenTypes[state];

							if (newType != TokenType.EOF)
							{
								type = newType;
								startOfNextToken = i + 1;
							}
						}
					}

					if (type != TokenType.EOF)
					{
						// if end is less than the last value of i then you will end up re-reading some characters,
						// if the start state is the only state that is not an end state then this will never happen.
						_currentToken = NewToken(type, _expressionString, _nextCharPosition, startOfNextToken - _nextCharPosition);
						_nextCharPosition = startOfNextToken;
					}
					else
					{
						// if you write your rules properly then this should never happen.
						// eg. add a rule at the end that matches a single instance of any character.
						throw new InvalidOperationException(string.Format("Got stuck at position {0}.", _nextCharPosition));
					}
				}
			}
			while (_currentToken == null);

			return true;
		}

		void IEnumerator.Reset()
		{
			_currentToken = null;
			_nextCharPosition = 0;
			_solIndicies.Clear();
		}

		#endregion

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		TToken _currentToken;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _nextCharPosition;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		int _started;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		List<int> _solIndicies = new List<int>();
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly string _expressionString;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly char[] _charClassificationBoundries;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ushort[] _charClassification;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ushort[] _transitionTable;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly TokenType[] _tokenTypes;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly TableCache _cache;

		class TableCache
		{
			protected TableCache()
			{
				_charClassificationBoundries = new char[50]
				{
					'\b', '\t', '\n', '\f', '\r', '\u001f', ' ', '!', '"', '#', '$', '\'', '(', ')', '*', '.',
					'/', '9', ':', ';', '<', '=', '>', '?', '@', 'Z', '[', '\\', '`', 'd', 'e', 'f',
					'g', 'h', 'i', 'k', 'l', 'm', 'n', 'q', 'r', 's', 't', 'u', 'x', 'y', 'z', '{',
					'|', '}',
				};
				_charClassification = new ushort[51]
				{
					1, 9, 0, 9, 0, 1, 9, 29, 30, 1, 23, 1, 5, 26, 7, 1,
					24, 16, 22, 3, 31, 8, 25, 2, 1, 19, 1, 6, 1, 19, 13, 19,
					12, 19, 11, 19, 20, 19, 14, 19, 10, 17, 21, 18, 19, 15, 19, 4,
					28, 27, 1,
				};
				_transitionTable = Expand("Buffalo.Core.Parser.Configuration.AutoConfigScanner.0.table");
				_tokenTypes = new TokenType[44]
				{
					TokenType.EOF,
					TokenType.Assign, // [=]
					TokenType.QuestionMark, // [?]
					TokenType.Bang, // [!]
					TokenType.Semicolon, // [;]
					TokenType.Pipe, // [|]
					TokenType.Becomes, // [:][:][=]
					TokenType.OpenBrace, // [{]
					TokenType.CloseBrace, // [}]
					TokenType.OpenParen, // [(]
					TokenType.CloseParen, // [)]
					TokenType.Null, // [n][u][l][l]
					TokenType.Using, // [u][s][i][n][g]
					TokenType.Entry, // [e][n][t][r][y]
					TokenType.TargetValue, // [$][$]
					TokenType.ArgumentValue, // [$][0-9]
					TokenType.Label, // [e][n][t]
					TokenType.Label, // [u][s][i]
					TokenType.Label, // [n][u][l]
					TokenType.Label, // [n][u]
					TokenType.Label, // [e][n]
					TokenType.Label, // [u][s]
					TokenType.Label, // [e]
					TokenType.Label, // [u]
					TokenType.Label, // [n]
					TokenType.Label, // [u][s][i][n]
					TokenType.Label, // [e][n][t][r]
					TokenType.Label, // [A-Z,a-d,f-m,o-t,v-z]
					TokenType.String, // ["]["]
					TokenType.BrokenString, // ["]
					TokenType.BrokenString, // ["][\\]
					TokenType.NonTerminal, // [<][A-Z,a-z][>]
					TokenType.BrokenNonTerminal, // [<][A-Z,a-z]
					TokenType.Comment, // [/][*][*][/]
					TokenType.Comment, // [/][/]
					TokenType.BrokenComment, // [/][*][*]
					TokenType.BrokenComment, // [/][*]
					TokenType.Whitespace, // [\t-\r, ]
					TokenType.Error, // [:][:]
					TokenType.Error, // ![\t-\r, -",$,(-),/,:-=,?,A-Z,a-}]
					TokenType.Error, // [<]
					TokenType.Error, // [:]
					TokenType.Error, // [/]
					TokenType.Error, // [$]
				};
			}

			public static TableCache Get()
			{
				TableCache result;

				lock (_weakRef)
				{
					if ((result = (TableCache)_weakRef.Target) == null)
					{
						_weakRef.Target = result = new TableCache();
					}
				}

				return result;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public readonly char[] _charClassificationBoundries;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public readonly ushort[] _charClassification;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public readonly ushort[] _transitionTable;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public readonly TokenType[] _tokenTypes;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			static readonly WeakReference _weakRef = new WeakReference(null);
		}
	}
}
