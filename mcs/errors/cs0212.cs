// cs0212: You can only take the address of an unfixed expression inside
//         a fixed statement initializer.
// Line: 17
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
