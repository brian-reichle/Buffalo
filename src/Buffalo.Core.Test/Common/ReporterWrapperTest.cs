// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using Moq;
using NUnit.Framework;

namespace Buffalo.Core.Common.Test
{
	[TestFixture]
	public class ReporterWrapperTest
	{
		[Test]
		public void AddError()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			var wrapper = new ReporterWrapper(reporter.Object);
			Assert.That(wrapper.HasError, Is.EqualTo(false));

			reporter.Setup(x => x.AddError(1, 2, 3, 4, "Text")).Verifiable();
			wrapper.AddError(1, 2, 3, 4, "Text");
			Assert.That(wrapper.HasError, Is.EqualTo(true));
			reporter.Verify();
		}

		[Test]
		public void AddWarning()
		{
			var reporter = new Mock<IErrorReporter>(MockBehavior.Strict);

			var wrapper = new ReporterWrapper(reporter.Object);
			Assert.That(wrapper.HasError, Is.EqualTo(false));

			reporter.Setup(x => x.AddWarning(1, 2, 3, 4, "Text")).Verifiable();
			wrapper.AddWarning(1, 2, 3, 4, "Text");
			Assert.That(wrapper.HasError, Is.EqualTo(false));
			reporter.Verify();
		}
	}
}
