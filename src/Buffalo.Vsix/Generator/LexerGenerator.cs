// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using Buffalo.Core;
using Microsoft.VisualStudio.Shell;

namespace Buffalo.Vsix
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
	[Guid("0876baa0-089d-11de-8c30-0800200c9a66")]
	[CodeGeneratorRegistration(typeof(LexerGenerator), "Buffalo Lexer", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true, GeneratorRegKeyName = "BuffaloLexer")]
	[ProvideObject(typeof(LexerGenerator), RegisterUsing = RegistrationMethod.CodeBase)]
	sealed class LexerGenerator : Generator
	{
		public LexerGenerator()
			: base(GeneratorType.Lexer)
		{
		}
	}
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
}
