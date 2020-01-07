// <auto-generated />
//------------------------------------------------------------------------------
// This code is auto-generated.
// Do not attempt to edit this file by hand, you could hurt yourself!
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Type_List = System.Collections.Generic.List<System.String>;
using Type_Token = Buffalo.Core.Test.Token;
using Type_Value = System.String;

namespace Buffalo.Generated.ErrorWithoutEOF
{
	internal enum TokenType
	{
		CloseParen = 3,
		Comma = 2,
		EOF = 0,
		Error = 4,
		Label = 1,
		OpenParen = 5,
	}

	internal abstract class AutoParser
	{
		// [Statistics]
		// Reductions          : 5
		// Terminals           : 6 (6 columns)
		// NonTerminals        : 3 (2 columns)
		// States              : 10
		//   Short Circuited   : 6
		//   With Goto Entries : 3
		//   With SR Conflicts : 0
		//   Other             : 1
		// Transition Table    : 32/80(40.00%)
		//   Primary Offsets   : 10
		//   Goto Offsets      : 3
		//   Actions           : 19
		// Memory Footprint    : 64 bytes
		// Assembly Footprint  : 64 bytes (100.00%)
		protected AutoParser()
		{
			_transitionTable = GetTransitionTable();
		}

		public Type_List Parse(IEnumerable<Type_Token> tokens)
		{
			return (Type_List)Parse(tokens, 0).value;
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

					if (type == TokenType.Error)
					{
						state = ReduceError(stack, enumerator);
					}
					else
					{
						int offset = _transitionTable[state];
						int action = offset <= 6 ? offset : _transitionTable[offset + (int)type];

						if (action > 6)
						{
							State newState = new State();
							newState.state = state = action - 6;
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
							state = ReduceError(stack, enumerator);
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
				case 3: // <Values> -> OpenParen <List> CloseParen
					newState.value = Reduce_Values_1((Type_Token)stack[stack.Count - 3].value, (Type_List)stack[stack.Count - 2].value, (Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveRange(stack.Count - 3, 3);
					break;

				case 4: // <List> -> <List> Comma <Element>
					newState.value = Reduce_List_1((Type_List)stack[stack.Count - 3].value, (Type_Token)stack[stack.Count - 2].value, (Type_Value)stack[stack.Count - 1].value);
					gotoIndex = 1;
					stack.RemoveRange(stack.Count - 3, 3);
					break;

				case 2: // <List> -> <Element>
					newState.value = Reduce_List_2((Type_Value)stack[stack.Count - 1].value);
					gotoIndex = 1;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 1: // <Element> -> Label
					newState.value = Reduce_Element_1((Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveAt(stack.Count - 1);
					break;

				case 0: // <Element> -> Error
					newState.value = Reduce_Element_2((Type_Token)stack[stack.Count - 1].value);
					gotoIndex = 0;
					stack.RemoveAt(stack.Count - 1);
					break;

				default: throw new InvalidOperationException("unknown reduction");
			}

			int state = stack[stack.Count - 1].state;
			newState.state = _transitionTable[_transitionTable[state + 10] + gotoIndex];

			stack.Add(newState);

			return newState.state;
		}

		int ReduceError(List<State> stack, IEnumerator<Type_Token> enumerator)
		{
			Type_Token errorToken = enumerator.Current;

			if (GetTokenType(enumerator.Current) != TokenType.EOF)
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("ran out of tokens while attempting to recover from a parse error.");
				}
			}

			bool[] failed = new bool[6];

			do
			{
				int state = stack[stack.Count - 1].state;
				int offset = _transitionTable[state];
				int action = offset <= 6 ? offset : _transitionTable[offset + (int)TokenType.Error];

				if (action == 0 || action > 6)
				{
					break;
				}
				else
				{
					int reductionId = action - 2;
					state = Reduce(reductionId, stack);
				}
			}
			while (true);

			do
			{
				TokenType nextType = GetTokenType(enumerator.Current);

				if (!failed[(int)nextType])
				{
					for (int i = stack.Count - 1; i >= 0; i--)
					{
						int state = stack[i].state;
						int offset = _transitionTable[state];
						if (offset <= 6) continue;

						int action = _transitionTable[offset + (int)TokenType.Error] - 6;

						if (action <= 0) continue;
						if (!CanBeFollowedBy(stack, i, action, nextType)) continue;

						State newState = new State();
						newState.state = action;
						newState.value = errorToken;

						stack.RemoveRange(i + 1, stack.Count - i - 1);
						stack.Add(newState);
						return action;
					}

					failed[(int)nextType] = true;
				}

				if (nextType == TokenType.EOF)
				{
					throw new InvalidOperationException("unexpected token: " + GetTokenType(errorToken));
				}
			}
			while (enumerator.MoveNext());

			throw new InvalidOperationException("ran out of tokens while attempting to recover from a parse error.");
		}

		bool CanBeFollowedBy(List<State> stack, int tosIndex, int startState, TokenType type)
		{
			List<int> overStack = new List<int>();
			overStack.Add(startState);

			int state = startState;

			while (true)
			{
				int offset = _transitionTable[state];
				int action = offset <= 6 ? offset : _transitionTable[offset + (int)type];

				if (action == 0) return false;
				if (action > 6 || action == 1) return true;

				int count;
				int gotoIndex;

				switch (action - 2)
				{
					case 3: // <Values> -> OpenParen <List> CloseParen
						count = 3;
						gotoIndex = 0;
						break;

					case 4: // <List> -> <List> Comma <Element>
						count = 3;
						gotoIndex = 1;
						break;

					case 2: // <List> -> <Element>
						count = 1;
						gotoIndex = 1;
						break;

					case 1: // <Element> -> Label
						count = 1;
						gotoIndex = 0;
						break;

					case 0: // <Element> -> Error
						count = 1;
						gotoIndex = 0;
						break;

					default: throw new InvalidOperationException("unknown reduction");
				}

				if (count >= overStack.Count)
				{
					tosIndex = tosIndex - count + overStack.Count;
					overStack.Clear();

					state = stack[tosIndex].state;
				}
				else if (count > 0)
				{
					overStack.RemoveRange(overStack.Count - count, count);

					state = overStack[overStack.Count - 1];
				}

				overStack.Add(state = _transitionTable[_transitionTable[state + 10] + gotoIndex]);
			}
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

		protected abstract Type_List Reduce_Values_1(Type_Token openParenSeg, Type_List listSeg, Type_Token closeParenSeg);
		protected abstract Type_List Reduce_List_1(Type_List listSeg, Type_Token commaSeg, Type_Value elementSeg);
		protected abstract Type_List Reduce_List_2(Type_Value elementSeg);
		protected abstract Type_Value Reduce_Element_1(Type_Token labelSeg);
		protected abstract Type_Value Reduce_Element_2(Type_Token errorSeg);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ushort[] _transitionTable;

		struct State
		{
			public int state;
			public object value;
		}

		static ushort[] GetTransitionTable()
		{
			return Extract("Buffalo.Core.Test.Parser.Generation.Gramour.AutoParserErrorWithoutEOF.table");
		}
	}
}