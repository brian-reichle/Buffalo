// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.ErrorWithoutEOF";
TokenType = Token;

using List = "System.Collections.Generic.List<System.String>";
using Token = "Buffalo.Core.Test.Token";
using Value = "System.String";

<Values> (List)
	::= OpenParen <List> CloseParen
	;

<List> (List)
	::= <List> Comma <Element>
	| <Element>
	;

<Element> (Value)
	::= Label
	| Error
	;
