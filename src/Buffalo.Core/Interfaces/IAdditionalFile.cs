// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.IO;

namespace Buffalo.Core
{
	public interface IAdditionalFile
	{
		string Suffix { get; }
		bool IsBinary { get; }
		AdditionalFileType Type { get; }

		void GenerateFileContent(TextWriter writer);
		void GenerateFileContent(Stream stream);
	}
}
