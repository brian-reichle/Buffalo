// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
Name = "AutoScanner";
Namespace = "Buffalo.Generated.CacheMultiEntry";
CacheTables = true;

state INITIAL
{
	^<%$					{ token StartOther; }
	^\-$					{ token Subtract; }
	^\*$					{ token Multiply; }
	^\+$					{ token Plus; }
	^\/$					{ token Divide; }

	^\($					{ token OpenParen; }
	^\)$					{ token CloseParen; }

	^cat$					{ token Cat; }
	^dog$					{ token Dog; }

	^[0-9]+(\.[0-9]+)?$		{ token Number; }
	^[a-zA-Z][a-zA-Z0-9]*$	{ token Label; }

	^[ \t\r\n\f\v]+$		{ token Whitespace; }
}

state Other
{
	^%>$					{ token EndOther; }
	^[a-zA-Z][a-zA-Z0-9]*$	{ token Label; }

	^[ \t\r\n\f\v]+$		{ token Whitespace; }
}
