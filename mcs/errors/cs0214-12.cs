// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 13
// Compiler options: -unsafe

class C
{
	public static unsafe void Write (params int*[] args)
	{
	}
	
	public static void Main ()
	{
		Write ();
	}
}
