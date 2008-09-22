// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 7
// Compiler options: -unsafe

class C
{
	int*[] data = new int*[16];
	
	unsafe C ()
	{
	}
}
