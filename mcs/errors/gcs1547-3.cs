// CS1547: Keyword `void' cannot be used in this context
// Line: 8
// Compiler options: -langversion:linq

class X
{
	static void Main ()
	{
		var e = from void v in new int [] { 0 } select i;
	}
}
