// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.ShiftReduceConflict";
TokenType = Token;

using Statement = "System.String";
using Token = "Buffalo.Core.Test.Token";

<Statement> (Statement)
	::= <If>
	| Other { $$ = null; }
	;

<If> (Statement)
	::= IfKeyword <Statement>
	| IfKeyword <Statement> ElseKeyword <Statement>
	;
