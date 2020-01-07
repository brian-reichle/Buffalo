// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoParser";
TypeName = "TokenType";
Namespace = "Buffalo.Generated.GotoOnlyStates";
TokenType = Token;

using Segment = "System.String";
using Token = "Buffalo.Core.Test.Token";

<L> (Segment)
	::= <L> A
	| <L> A B
	|
	;
