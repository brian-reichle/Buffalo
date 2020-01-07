// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Buffalo.Core.Lexer
{
	sealed class LexerTableFile : IAdditionalFile
	{
		public LexerTableFile(int tableID, CodeGen codeGen)
		{
			_tableID = tableID;
			_codeGen = codeGen;
			Suffix = "." + tableID + ".table";
		}

		public string Suffix { get; }
		public void GenerateFileContent(Stream stream) => _codeGen.WriteTableResource(stream, _tableID);

		AdditionalFileType IAdditionalFile.Type => AdditionalFileType.EmbeddedResource;
		bool IAdditionalFile.IsBinary => true;
		void IAdditionalFile.GenerateFileContent(TextWriter writer) => throw new NotSupportedException();

		readonly int _tableID;
		readonly CodeGen _codeGen;
	}
}
