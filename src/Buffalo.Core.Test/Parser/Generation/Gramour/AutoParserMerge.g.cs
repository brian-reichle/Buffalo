// <auto-generated />
//------------------------------------------------------------------------------
// This code is auto-generated.
// Do not attempt to edit this file by hand, you could hurt yourself!
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Type_Token = Buffalo.Core.Test.Token;

namespace Buffalo.Generated.Merge
{
	internal enum TokenType
	{
		Alpha = 2,
		Beta = 2 + (1 << 3),
		Close = 0,
		Delta = 2 + (2 << 3),
		EOF = 1,
		Gamma = 2 + (3 << 3),
		Open = 3,
	}

	internal abstract class AutoParser
	{
		const TokenType ColumnMask = (TokenType)7;

		// [Statistics]
		// Reductions          : 3
		// Terminals           : 7 (4 columns)
		// NonTerminals        : 2 (2 columns)
		// States              : 7
		//   Short Circuited   : 4
		//   With Goto Entries : 1
		//   With SR Conflicts : 0
		//   Other             : 2
		// Transition Table    : 20/42(47.62%)
		//   Primary Offsets   : 7
		//   Goto Offsets      : 1
		//   Actions           : 12
		// Memory Footprint    : 40 bytes
		// Assembly Footprint  : 40 bytes (100.00%)
		protected AutoParser()
		{
			_transitionTable = GetTransitionTable();
		}

		public Type_Token Parse(IEnumerable<Type_Token> tokens)
		{
			return (Type_Token)Parse(tokens, 0).value;
		}

		State Parse(IEnumerable<Type_Token> tokens, int initialState)
		{
			List<State> stack = new List<State>();

			{
				State init = new State();
				init.state = initialState;
				stack.Add(init);
			}

			int state = initialState;

			using (IEnumerator<Type_Token> enumerator = tokens.GetEnumerator())
			{
				bool haveToken = enumerator.MoveNext();

				while (haveToken)
				{
					Type_Token token = enumerator.Current;
					TokenType type = GetTokenType(token);

					{
						int offset = _transitionTable[state];
						int action = offset <= 4 ? offset : _transitionTable[offset + (int)(type & ColumnMask)];

						if (action > 4)
						{
							State newState = new State();
							newState.state = state = action - 4;
							newState.value = token;

							stack.Add(newState);
							haveToken = enumerator.MoveNext();
						}
						else if (action > 1)
						{
							int reductionId = action - 2;
							state = Reduce(reductionId, stack);
						}
						else if (action == 0 || type != TokenType.EOF)
						{
							UnexpectedToken(token);
							throw new InvalidOperationException("unexpected token: " + type);
						}
						else
						{
							return stack[1];
						}
					}
				}
			}

			throw new InvalidOperationException("ran out of tokens, somehow");
		}

		int Reduce(int reductionId, List<State> stack)
		{
			State newState = new State();
			int gotoIndex;

			switch (reductionId)
			{
				case 2: // <Document> -> Open <Mid> Close
					newState.value = (Type_Token)stack[stack.Count - 2].value;
					gotoIndex = 0;
					stack.RemoveRange(stack.Count - 3, 3);
					break;

				case 1: // <Document> -> <Alt>
					newState.value = Reduce_Document_2((Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 0: // <Alt> -> <Mid>
					newState.value = Reduce_Alt_1((Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 1;
					stack.RemoveAt(stack.Count - 1);
					break;

				default: throw new InvalidOperationException("unknown reduction");
			}

			int state = stack[stack.Count - 1].state;
			newState.state = _transitionTable[_transitionTable[state + 7] + gotoIndex];

			stack.Add(newState);

			return newState.state;
		}

		protected virtual void UnexpectedToken(Type_Token terminal)
		{
		}

		static ushort[] Extract(string resourceName)
		{
			using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				int len = (int)stream.Length;
				ushort[] result = new ushort[len >> 1];
				byte[] buffer = new byte[Math.Min(stream.Length, 512)];
				int offset = 0;
				int read;

				do
				{
					read = stream.Read(buffer, 0, buffer.Length);
					Buffer.BlockCopy(buffer, 0, result, offset, read);
					offset += read;
				}
				while (offset < len && read > 0);

				return result;
			}
		}

		protected abstract TokenType GetTokenType(Type_Token terminal);

		protected abstract Type_Token Reduce_Document_2(Type_Token altSeg);
		protected abstract Type_Token Reduce_Alt_1(Type_Token midSeg);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ushort[] _transitionTable;

		struct State
		{
			public int state;
			public object value;
		}

		static ushort[] GetTransitionTable()
		{
			return Extract("Buffalo.Core.Test.Parser.Generation.Gramour.AutoParserMerge.table");
		}
	}
}