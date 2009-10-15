// CS214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 21
// Compiler options: -unsafe

public unsafe delegate int* Bar ();

class X
{
	unsafe static int* Test ()
	{
		return null;
	}

	static void Main ()
	{
		Bar b;
		unsafe {
			b = Test;
		}
		
		b ();
	}
}
