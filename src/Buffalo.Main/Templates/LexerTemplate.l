Name = "AutoScanner";
Namespace = "Namespace";

state INITIAL
{
	^[ \t\r\n\f\v\b]+$
	{
		token Whitespace;
	}

	^.$
	{
		token Error;
	}
}
