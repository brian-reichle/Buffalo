// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.Merge";
TokenType = Token;
RenderParseTable = true;
TrimParseGraph = true;

using Token = "Buffalo.Core.Test.Token";

<Document> (Token)
	::= Open <Mid> Close { $$ = $1; }
	| <Alt>
	;

<Alt> (Token)
	::= <Mid>
	;

<Mid> (Token)
	::= Alpha { $$ = $0; }
	| Beta { $$ = $0; }
	| <SubMid> { $$ = $0; }
	;

<SubMid> (Token)
	::= Gamma { $$ = $0; }
	| Delta { $$ = $0; }
	;
