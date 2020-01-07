// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Globalization;

namespace Buffalo.Core.Lexer
{
	struct NodeData
	{
		public NodeData(int? startState, int? endState)
			: this(startState, endState, 0)
		{
		}

		public NodeData(int? startState, int? endState, int priority)
		{
			StartState = startState;
			EndState = endState;
			Priority = priority;
		}

		public int? StartState { get; }
		public int? EndState { get; }
		public int Priority { get; }

		public override string ToString()
		{
			string format;

			if (StartState.HasValue)
			{
				format = EndState.HasValue ? "S{0} E{1}" : "S{0}";
			}
			else
			{
				format = EndState.HasValue ? "E{1}" : string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, format, StartState, EndState, Priority);
		}
	}
}
