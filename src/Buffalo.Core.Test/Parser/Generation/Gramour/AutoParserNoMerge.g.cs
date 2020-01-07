// <auto-generated />
//------------------------------------------------------------------------------
// This code is auto-generated.
// Do not attempt to edit this file by hand, you could hurt yourself!
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Type_Token = Buffalo.Core.Test.Token;

namespace Buffalo.Generated.NoMerge
{
	internal enum TokenType
	{
		Alpha = 0,
		Beta = 5,
		Close = 2,
		Delta = 1,
		EOF = 3,
		Gamma = 4,
		Open = 6,
	}

	internal abstract class AutoParser
	{
		// [Statistics]
		// Reductions          : 8
		// Terminals           : 7 (7 columns)
		// NonTerminals        : 4 (4 columns)
		// States              : 12
		//   Short Circuited   : 9
		//   With Goto Entries : 2
		//   With SR Conflicts : 0
		//   Other             : 1
		// Transition Table    : 39/132(29.55%)
		//   Primary Offsets   : 12
		//   Goto Offsets      : 2
		//   Actions           : 25
		// Memory Footprint    : 78 bytes
		// Assembly Footprint  : 78 bytes (100.00%)
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
						int action = offset <= 9 ? offset : _transitionTable[offset + (int)type];

						if (action > 9)
						{
							State newState = new State();
							newState.state = state = action - 9;
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
				case 7: // <Document> -> Open <Mid> Close
					newState.value = (Type_Token)stack[stack.Count - 2].value;
					gotoIndex = 2;
					stack.RemoveRange(stack.Count - 3, 3);
					break;

				case 1: // <Document> -> <Alt>
					newState.value = Reduce_Document_2((Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 2;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 0: // <Alt> -> <Mid>
					newState.value = Reduce_Alt_1((Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 3;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 2: // <Mid> -> Alpha
					newState.value = (Type_Token)stack[stack.Count - 1].value;
					gotoIndex = 0;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 3: // <Mid> -> Beta
					newState.value = (Type_Token)stack[stack.Count - 1].value;
					gotoIndex = 0;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 4: // <Mid> -> <SubMid>
					newState.value = (Type_Token)stack[stack.Count - 1].value;
					gotoIndex = 0;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 6: // <SubMid> -> Gamma
					newState.value = (Type_Token)stack[stack.Count - 1].value;
					gotoIndex = 1;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 5: // <SubMid> -> Delta
					newState.value = (Type_Token)stack[stack.Count - 1].value;
					gotoIndex = 1;
					stack.RemoveAt(stack.Count - 1);
					break;

				default: throw new InvalidOperationException("unknown reduction");
			}

			int state = stack[stack.Count - 1].state;
			newState.state = _transitionTable[_transitionTable[state + 12] + gotoIndex];

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
			return Extract("Buffalo.Core.Test.Parser.Generation.Gramour.AutoParserNoMerge.table");
		}
	}
}