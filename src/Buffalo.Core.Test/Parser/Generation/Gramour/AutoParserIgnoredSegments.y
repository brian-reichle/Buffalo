// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.IgnoredSegments";
TokenType = Token;

using Token = "Buffalo.Core.Test.Token";
using Value = "System.String";

<Document> (Value)
	::= A <A>!
	| B B!
	;

<A> (Token)
	::= A { $$ = $0; }
	;
