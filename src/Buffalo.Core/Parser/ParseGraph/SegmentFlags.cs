// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Core.Parser
{
	[Flags]
	enum SegmentFlags
	{
		None = 0x00,
		IsTerminal = 0x01,
		IsBaseTerminal = 0x02,
		Optional = 0x04,
		IsInitial = 0x08,
	}
}
