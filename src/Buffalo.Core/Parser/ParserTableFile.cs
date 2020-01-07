// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Parser
{
	sealed class ParserTableFile : IAdditionalFile
	{
		public const string Suffix = ".table";

		public ParserTableFile(CodeGen codeGen)
		{
			if (codeGen == null) throw new ArgumentNullException(nameof(codeGen));
			_codeGen = codeGen;
		}

		public void GenerateFileContent(Stream stream) => _codeGen.WriteTableResource(stream);

		string IAdditionalFile.Suffix => Suffix;
		AdditionalFileType IAdditionalFile.Type => AdditionalFileType.EmbeddedResource;
		bool IAdditionalFile.IsBinary => true;

		void IAdditionalFile.GenerateFileContent(TextWriter writer) => throw new NotSupportedException();

		readonly CodeGen _codeGen;
	}
}
