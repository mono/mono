// Compiler options: -unsafe

using System;

unsafe class C
{
	public static void Main ()
	{
		int x = 5;
		int* a = &(*(x + (int*)null));
		int* b = &(*(x + (int*)1));
	}
}
