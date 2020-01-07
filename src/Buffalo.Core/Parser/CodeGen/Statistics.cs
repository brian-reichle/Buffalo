// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Parser
{
	sealed class Statistics
	{
		public int Reductions { get; set; }
		public int Terminals { get; set; }
		public int TerminalColumns { get; set; }
		public int NonTerminals { get; set; }
		public int NonTerminalColumns { get; set; }
		public int States { get; set; }
		public int StatesWithGotos { get; set; }
		public int StatesShortCircuited { get; set; }
		public int StatesWithSRConflicts { get; set; }
		public int StatesOther { get; set; }
		public int ActionsRunTime { get; set; }
		public int ActionsAssemblyBytes { get; set; }
		public int GotoOffsetsLen { get; set; }
	}
}
