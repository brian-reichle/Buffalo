// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Diagnostics;

namespace Buffalo.Main
{
	sealed class CodePage : ResultPage
	{
		public CodePage(GenerationManager manager, ConfigPage config)
			: base(manager, config)
		{
			_codeText = string.Empty;
		}

		public string CodeText
		{
			[DebuggerStepThrough]
			get => _codeText;
			set => SetField(ref _codeText, value);
		}

		string _codeText;
	}
}
