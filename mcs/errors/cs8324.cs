// CS8324: Named argument specifications must appear after all fixed arguments have been specified in a dynamic invocation
// Line: 10
// Compiler options: -langversion:7.2

class C
{
	void M ()
	{
		dynamic d = new object ();
		d.M (arg: 1, "");
	}
}