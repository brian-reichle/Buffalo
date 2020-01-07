// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Diagnostics;

namespace Buffalo.Core.Common
{
	[DebuggerDisplay("({Index}, {LineNo}, {CharNo})")]
	sealed class CharPos
	{
		public CharPos(int index, int lineNo, int charNo)
		{
			Index = index;
			LineNo = lineNo;
			CharNo = charNo;
		}

		public int Index { get; }
		public int LineNo { get; }
		public int CharNo { get; }
	}
}
