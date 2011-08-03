// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 13
// Compiler options: -unsafe

struct X {
	static unsafe void *a ()
		{
			return null;
		}

	static void Main ()
		{
			a ();
		}
	
}
