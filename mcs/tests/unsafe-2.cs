//
// This test excercises stackalloc, some pointer arithmetic,
// and dereferences
//
using System;
unsafe class X {
	static int Main ()
	{
		char *ptr = stackalloc char [10];
		int i;
		
		for (i = 0; i < 10; i++)
			ptr [i] = (char) (i + 10);

		for (i = 0; i < 10; i++){
			if (*ptr != (char) (i + 10))
				return 200 + i;
			ptr++;
		}
		Console.WriteLine ("Ok");
		return 0;
	}
}	


