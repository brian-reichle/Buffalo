// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Core
{
	public interface IErrorReporter
	{
		void AddError(int fromLine, int fromChar, int toLine, int toChar, string text);
		void AddWarning(int fromLine, int fromChar, int toLine, int toChar, string text);
	}
}
