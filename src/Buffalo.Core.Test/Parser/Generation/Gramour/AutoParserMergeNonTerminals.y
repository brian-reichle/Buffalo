// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.MergeNonTerminals";
TokenType = Token;
TrimParseGraph = true;

using Token = "Buffalo.Core.Test.Token";

<Document>
	::= Pre <A> Post
	;

<A>
	::= <B1> { $$ = $0; }
	| <B2> { $$ = $0; }
	;

<B1>
	::= X1 Y1
	;

<B2>
	::= X2 Y2
	;

