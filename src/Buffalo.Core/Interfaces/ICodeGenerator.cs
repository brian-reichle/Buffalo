// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.IO;

namespace Buffalo.Core
{
	public interface ICodeGenerator
	{
		bool Load(TextReader reader, IErrorReporter reporter, ICodeGeneratorEnv environment);
		void Write(TextWriter writer);
		void ReportPerformance(IPerformanceReporter reporter);

		IEnumerable<IAdditionalFile> AdditionalFiles { get; }
	}
}
