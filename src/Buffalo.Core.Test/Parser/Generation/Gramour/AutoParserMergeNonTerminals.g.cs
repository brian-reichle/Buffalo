// <auto-generated />
//------------------------------------------------------------------------------
// This code is auto-generated.
// Do not attempt to edit this file by hand, you could hurt yourself!
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Type_Token = Buffalo.Core.Test.Token;
using Type_Object = System.Object;

namespace Buffalo.Generated.MergeNonTerminals
{
	internal enum TokenType
	{
		EOF = 2,
		Post = 3,
		Pre = 6,
		X1 = 0,
		X2 = 5,
		Y1 = 1,
		Y2 = 4,
	}

	internal abstract class AutoParser
	{
		// [Statistics]
		// Reductions          : 3
		// Terminals           : 7 (7 columns)
		// NonTerminals        : 3 (1 column)
		// States              : 9
		//   Short Circuited   : 4
		//   With Goto Entries : 2
		//   With SR Conflicts : 0
		//   Other             : 3
		// Transition Table    : 37/72(51.39%)
		//   Primary Offsets   : 9
		//   Goto Offsets      : 2
		//   Actions           : 26
		// Memory Footprint    : 74 bytes
		// Assembly Footprint  : 68 bytes (91.89%)
		protected AutoParser()
		{
			_transitionTable = GetTransitionTable();
		}

		public Type_Object Parse(IEnumerable<Type_Token> tokens)
		{
			return (Type_Object)Parse(tokens, 0).value;
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
						int action = offset <= 4 ? offset : _transitionTable[offset + (int)type];

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
				case 2: // <Document> -> Pre <A> Post
					newState.value = Reduce_Document_1((Type_Token)stack[stack.Count - 3].value, (Type_Object)stack[stack.Count - 2].value, (Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveRange(stack.Count - 3, 3);
					break;

				case 0: // <B1> -> X1 Y1
					newState.value = Reduce_B1_1((Type_Token)stack[stack.Count - 2].value, (Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveRange(stack.Count - 2, 2);
					break;

				case 1: // <B2> -> X2 Y2
					newState.value = Reduce_B2_1((Type_Token)stack[stack.Count - 2].value, (Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveRange(stack.Count - 2, 2);
					break;

				default: throw new InvalidOperationException("unknown reduction");
			}

			int state = stack[stack.Count - 1].state;
			newState.state = _transitionTable[_transitionTable[state + 9] + gotoIndex];

			stack.Add(newState);

			return newState.state;
		}

		protected virtual void UnexpectedToken(Type_Token terminal)
		{
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

		protected abstract TokenType GetTokenType(Type_Token terminal);

		protected abstract Type_Object Reduce_Document_1(Type_Token preSeg, Type_Object aSeg, Type_Token postSeg);
		protected abstract Type_Object Reduce_B1_1(Type_Token x1Seg, Type_Token y1Seg);
		protected abstract Type_Object Reduce_B2_1(Type_Token x2Seg, Type_Token y2Seg);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ushort[] _transitionTable;

		struct State
		{
			public int state;
			public object value;
		}

		static ushort[] GetTransitionTable()
		{
			return Expand("Buffalo.Core.Test.Parser.Generation.Gramour.AutoParserMergeNonTerminals.table");
		}
	}
}
