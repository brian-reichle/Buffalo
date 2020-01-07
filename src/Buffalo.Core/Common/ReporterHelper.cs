// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Globalization;

namespace Buffalo.Core.Common
{
	sealed class ReporterHelper
	{
		public static void AddError(IErrorReporter reporter, ICharRange token, string text)
		{
			var fromPos = token.FromPos;
			var toPos = token.ToPos;

			reporter.AddError(
				fromPos.LineNo,
				fromPos.CharNo,
				toPos.LineNo,
				toPos.CharNo,
				text);
		}

		public static void AddError(IErrorReporter reporter, ICharRange token, string format, params object[] args)
		{
			AddError(reporter, token, string.Format(CultureInfo.CurrentCulture, format, args));
		}

		public static void AddWarning(IErrorReporter reporter, ICharRange token, string text)
		{
			var fromPos = token.FromPos;
			var toPos = token.ToPos;

			reporter.AddWarning(
				fromPos.LineNo,
				fromPos.CharNo,
				toPos.LineNo,
				toPos.CharNo,
				text);
		}

		public static void AddWarning(IErrorReporter reporter, ICharRange token, string format, params object[] args)
		{
			AddWarning(reporter, token, string.Format(CultureInfo.CurrentCulture, format, args));
		}
	}
}
