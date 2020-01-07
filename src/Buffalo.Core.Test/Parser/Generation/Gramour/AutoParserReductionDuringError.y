// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.ReductionDuringError";
TokenType = Token;

using Segment = "System.String";
using Token = "Buffalo.Core.Test.Token";

<Test> (Segment)
	::= <A> <B> <C>
	;

<A> (Segment)
	::= <X> <Y>
	;

<B> (Segment)
	::= <X> <Y>
	;

<C> (Segment)
	::= <X> <Y>
	;

<X> (Segment)
	::= x
	| Error
	|
	;

<Y> (Segment)
	::= y
	|
	;
