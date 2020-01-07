// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoConfigScanner";
Namespace = "Buffalo.Core.Lexer.Configuration";
Visibility = Internal;
CacheTables = true;
TableCompression = CTB;

state INITIAL
{
	// Symbols
	^\{$						{ token OpenBrace; }
	^\}$						{ token CloseBrace; }
	^;$							{ token Semicolon; }
	^=$							{ token Assign; }

	// Keywords
	^state$						{ token State; }
	^token$						{ token Token; }

	// Values
	^[A-Za-z][A-Za-z0-9]*$		{ token Label; }
	^"(\\.|[^\\"\r\n])*"$		{ token String; }
	^"(\\.|[^\\"\r\n])*\\?$		{ token BrokenString; }
	^\^(\\.|[^\$\r\n\\])*\$$	{ token Regex; }
	^\^(\\.|[^\$\r\n\\])*\\?$	{ token BrokenRegex; }

	// Ignored
	^//.*$						{ token Comment; }
	^/\*([^*]|[*][^/])*\*/$		{ token Comment; }
	^/\*([^*]|[*][^/])*\*?$		{ token BrokenComment; }
	^[\r\n\v\t\f ]+$			{ token Whitespace; }

	// Catch All
	^.$							{ token Error; }
}
