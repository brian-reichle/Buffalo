// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Core.Common
{
	static class PerfReporterHelper
	{
		public static void AddPerfMetric(IPerformanceReporter reporter, string name, DateTime? from, DateTime? to)
		{
			if (from.HasValue && to.HasValue)
			{
				reporter.AddPerformanceMetric(name, to.Value - from.Value);
			}
		}
	}
}
