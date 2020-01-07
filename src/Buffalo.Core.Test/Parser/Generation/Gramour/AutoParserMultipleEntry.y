// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.MultipleEntry";
TokenType = Token;

using Bool = "System.Boolean";
using Int = "System.Int32";
using Token = "Buffalo.Core.Test.Token";

entry <Equation>;
entry <Expression>;

<Equation> (Bool)
	::= <Expression> EqualTo <Expression>
	;

<Expression> (Int)
	::= <Expression> <TermOp> <Term>
	| <Term> { $$ = $0; }
	;

<TermOp> (Token)
	::= Add { $$ = $0; }
	| Subtract { $$ = $0; }
	;

<Term> (Int)
	::= <Term> <FactorOp> <Factor>
	| <Factor> { $$ = $0; }
	;

<FactorOp> (Token)
	::= Multiply { $$ = $0; }
	| Divide { $$ = $0; }
	;

<Factor> (Int)
	::= Number
	| OpenParen <Expression> CloseParen { $$ = $1; }
	;
