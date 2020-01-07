// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public class PerfReporterHelperTest
	{
		[Test]
		public void AddPerfMetric()
		{
			var reporter = new Mock<IPerformanceReporter>(MockBehavior.Strict);

			PerfReporterHelper.AddPerfMetric(reporter.Object, "Test1", null, null);
			PerfReporterHelper.AddPerfMetric(reporter.Object, "Test2", DateTime.Now, null);
			PerfReporterHelper.AddPerfMetric(reporter.Object, "Test3", null, DateTime.Now);

			var now = DateTime.Now;
			reporter.Setup(x => x.AddPerformanceMetric("Test4", new TimeSpan(0, 0, 0, 0, 400))).Verifiable();
			PerfReporterHelper.AddPerfMetric(reporter.Object, "Test4", now, now.AddSeconds(0.4));
			reporter.Verify();
		}
	}
}
