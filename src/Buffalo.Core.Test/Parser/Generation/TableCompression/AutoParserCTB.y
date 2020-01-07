// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.CTB";
TokenType = Token;
TableCompression = CTB;

using Token = "Buffalo.Core.Test.Token";

<Equation>
	::= <Expression> <ComparisonOp> <Expression>
	;

<ComparisonOp>
	::= EqualTo
	| NotEqualTo
	| LessThan
	| LessThanOrEqualTo
	| GreaterThan
	| GreaterThanOrEqualTo
	;

<Expression>
	::= <Expression> <TermOp> <Term>
	| <Term>
	;

<TermOp>
	::= Add
	| Subtract
	;

<Term>
	::= <Term> <FactorOp> <Factor>
	| <Factor>
	;

<FactorOp>
	::= Multiply
	| Divide
	;

<Factor>
	::= <Value> Power <Value>
	| <Value>
	;

<Value>
	::= Number
	| OpenParen <Expression> CloseParen
	;
