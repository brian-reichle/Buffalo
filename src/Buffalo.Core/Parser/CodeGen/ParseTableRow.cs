// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Buffalo.Core.Common;

namespace Buffalo.Core.Parser
{
	sealed class ParseTableRow
	{
		public ParseTableRow(TableFragment fragment, bool hasReduction, bool hasShift)
		{
			Fragment = fragment;
			ShortCircuitReduction = 0;
			HasReduction = hasReduction;
			HasShift = hasShift;
		}

		public ParseTableRow(int shortCircuitReduction)
		{
			Fragment = null;
			ShortCircuitReduction = shortCircuitReduction;
			HasReduction = true;
			HasShift = false;
		}

		public TableFragment Fragment { get; }
		public int ShortCircuitReduction { get; }
		public bool HasReduction { get; }
		public bool HasShift { get; }
	}
}
