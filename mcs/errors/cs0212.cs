// CS0212: You can only take the address of unfixed expression inside of a fixed statement initializer
// Line: 19
// Compiler options: -unsafe

using System;

class X
{
	public int x;
	public X ()
	{
		this.x = 4;
	}

	public unsafe static void Main ()
	{
		X x = new X ();
		int *p = &x.x;
	}
}
