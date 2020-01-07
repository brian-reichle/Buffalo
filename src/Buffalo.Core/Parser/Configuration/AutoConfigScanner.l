// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoConfigScanner";
Namespace = "Buffalo.Core.Parser.Configuration";
Visibility = Internal;
CacheTables = true;
TableCompression = CTB;

state INITIAL
{
	// Symbols
	^=$							{ token Assign; }
	^\?$						{ token QuestionMark; }
	^!$							{ token Bang; }
	^;$							{ token Semicolon; }
	^\|$						{ token Pipe; }
	^::=$						{ token Becomes; }
	^\{$						{ token OpenBrace; }
	^\}$						{ token CloseBrace; }
	^\($						{ token OpenParen; }
	^\)$						{ token CloseParen; }

	// Keywords
	^null$						{ token Null; }
	^using$						{ token Using; }
	^entry$						{ token Entry; }

	// Values
	^\$\$$						{ token TargetValue; }
	^\$[0-9]+$					{ token ArgumentValue; }
	^[A-Za-z][A-Za-z0-9]*$		{ token Label; }
	^"(\\.|[^\\"\r\n])*"$		{ token String; }
	^"(\\.|[^\\"\r\n])*\\?$		{ token BrokenString; }
	^<[A-Za-z][A-Za-z0-9]*>$	{ token NonTerminal; }
	^<[A-Za-z][A-Za-z0-9]*$		{ token BrokenNonTerminal; }

	// Ignored
	^//[^\r\n]*$				{ token Comment; }
	^/\*([^*]|[*][^/])*\*/$		{ token Comment; }
	^/\*([^*]|[*][^/])*\*?$		{ token BrokenComment; }
	^[\r\n\v\t\f ]+$			{ token Whitespace; }

	// Catch All
	^.|::$						{ token Error; }
}
