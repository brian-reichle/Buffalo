// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace Buffalo.Main
{
	sealed class CodeResultToken : ResultToken<CodePage>
	{
		public CodeResultToken(string code, string suffix)
			: base(suffix)
		{
			Code = code;
		}

		public string Code { get; }

		protected override bool IsEmpty => string.IsNullOrEmpty(Code);

		protected override CodePage NewResultPage(ConfigPage page)
			=> new CodePage(page.Manager, page);

		protected override void UpdatePage(CodePage resultPage)
		{
			resultPage.CodeText = Code;
		}
	}
}
