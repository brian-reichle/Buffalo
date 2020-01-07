// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core.Lexer
{
	sealed class Statistics
	{
		public int CharClassifications { get; set; }
		public int CharRanges { get; set; }
		public int States { get; set; }
		public int StatesTerminal { get; set; }
		public int TransitionsRunTime { get; set; }
		public int TransitionsAssemblyBytes { get; set; }
	}
}
