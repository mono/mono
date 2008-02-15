// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 12
// Compiler options: -unsafe

using System;

class X {
	public unsafe int* A { set { } }
	
	void Foo ()
	{
		A = null;
	}

	static void Main () {}
}
