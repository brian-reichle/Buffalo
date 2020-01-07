// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using Buffalo.Core;
using Microsoft.VisualStudio.Shell;

namespace Buffalo.Vsix
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
	[Guid("a2b0c920-3c3d-11de-8a39-0800200c9a66")]
	[CodeGeneratorRegistration(typeof(ParserGenerator), "Buffalo Parser", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true, GeneratorRegKeyName = "BuffaloParser")]
	[ProvideObject(typeof(ParserGenerator), RegisterUsing = RegistrationMethod.CodeBase)]
	sealed class ParserGenerator : Generator
	{
		public ParserGenerator()
			: base(GeneratorType.Parser)
		{
		}
	}
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
}
