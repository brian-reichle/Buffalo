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

namespace Buffalo.Generated.Short
{
	internal abstract class AutoScanner<TToken> : IEnumerable<TToken>, IEnumerator<TToken>
		where TToken : class
	{
		public enum TokenType : ushort
		{
			EOF = 0,
			Subtract = 1,
			Multiply = 2,
			Plus = 3,
			Divide = 4,
			OpenParen = 5,
			CloseParen = 6,
			Cat = 7,
			Dog = 8,
			Number = 9,
			Label = 10,
			Whitespace = 11,
		}

		// [Statistics]
		// Char Classifications : 17
		// Char Ranges          : 29
		// States               : 18
		//   Terminal           : 6
		// Transition Table     : 138/306 (45.10%)
		//   Offsets            : 18
		//   Actions            : 120
		// Memory Footprint     : 428 bytes
		//   Boundries          : 58 bytes
		//   Classifications    : 58 bytes
		//   Transitions        : 276 bytes
		//   Token Types        : 36 bytes
		// Assembly Footprint   : 330 bytes (77.10%)
		protected AutoScanner(string expressionString)
		{
			_charClassificationBoundries = new char[28]
			{
				'\b', '\r', '\u001f', ' ', '\'', '(', ')', '*', '+', ',', '-', '.', '/', '9', '@', 'Z',
				'`', 'a', 'b', 'c', 'd', 'f', 'g', 'n', 'o', 's', 't', 'z',
			};
			_charClassification = new ushort[29]
			{
				0, 4, 0, 4, 0, 15, 1, 3, 14, 0, 2, 16, 13, 8, 0, 9,
				0, 6, 9, 12, 5, 9, 11, 9, 10, 9, 7, 9, 0,
			};
			_transitionTable = Expand("Buffalo.Core.Test.Lexer.Generation.ElementSize.Short.0.table");
			_tokenTypes = new TokenType[18]
			{
				TokenType.EOF,
				TokenType.EOF, // [0-9][.]
				TokenType.Subtract, // [-]
				TokenType.Multiply, // [*]
				TokenType.Plus, // [+]
				TokenType.Divide, // [/]
				TokenType.OpenParen, // [(]
				TokenType.CloseParen, // [)]
				TokenType.Cat, // [c][a][t]
				TokenType.Dog, // [d][o][g]
				TokenType.Number, // [0-9][.][0-9]
				TokenType.Number, // [0-9]
				TokenType.Label, // [c][a]
				TokenType.Label, // [d][o]
				TokenType.Label, // [A-Z,a-b,e-z]
				TokenType.Label, // [c]
				TokenType.Label, // [d]
				TokenType.Whitespace, // [\t-\r, ]
			};

			_expressionString = expressionString;
		}

		protected AutoScanner(AutoScanner<TToken> source)
		{
			_charClassificationBoundries = source._charClassificationBoundries;
			_charClassification = source._charClassification;
			_transitionTable = source._transitionTable;
			_tokenTypes = source._tokenTypes;

			_expressionString = source._expressionString;
		}

		~AutoScanner()
		{
			Dispose(false);
		}

		protected abstract TToken NewToken(TokenType type, string expressionString, int startPosition, int length);

		protected virtual AutoScanner<TToken> NewScanner()
		{
			AutoScanner<TToken> result = (AutoScanner<TToken>)MemberwiseClone();
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
			using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
			{
				ushort[] result = new ushort[reader.ReadUInt16()];
				ushort escape = reader.ReadUInt16();

				int w = 0;
				while (stream.Position < stream.Length)
				{
					ushort value = reader.ReadUInt16();

					if (value == escape)
					{
						ushort count = reader.ReadUInt16();
						value = reader.ReadUInt16();

						while (count > 0)
						{
							result[w++] = value;
							count--;
						}
					}
					else
					{
						result[w++] = value;
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
	}
}
