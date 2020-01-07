// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Core
{
	public interface IPerformanceReporter
	{
		void AddPerformanceMetric(string name, TimeSpan span);
	}
}
