// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;

namespace Buffalo.Core.Common
{
	sealed class ReporterWrapper : IErrorReporter
	{
		public ReporterWrapper(IErrorReporter innerReporter)
		{
			if (innerReporter == null) throw new ArgumentNullException(nameof(innerReporter));
			_innerReporter = innerReporter;
		}

		public void AddError(int fromLine, int fromChar, int toLine, int toChar, string text)
		{
			_innerReporter.AddError(fromLine, fromChar, toLine, toChar, text);
			HasError = true;
		}

		public void AddWarning(int fromLine, int fromChar, int toLine, int toChar, string text)
		{
			_innerReporter.AddWarning(fromLine, fromChar, toLine, toChar, text);
		}

		public bool HasError { get; private set; }

		readonly IErrorReporter _innerReporter;
	}
}
