// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 17
// Compiler options: -unsafe

public unsafe delegate void Bar (int* x);

class X
{
	public X (Bar bar)
	{ }

	unsafe static void Test (int* b)
	{ }

	static void Main ()
	{
		X x = new X (Test);
	}
}
