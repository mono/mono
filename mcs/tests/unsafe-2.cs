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
		long l = 0;
		ulong ul = 0;
		byte b = 0;
		
		for (i = 0; i < 10; i++)
			ptr [i] = (char) (i + 10);

		for (i = 0; i < 10; i++){
			if (*ptr != (char) (i + 10))
				return 200 + i;
			ptr++;
		}


		// Now test index access with longs
		if (ptr [l] != 10)
			return 1;
		if (ptr [ul] != 10)
			return 2;
		if (ptr [b] != 10)
			return 3;
		
		Console.WriteLine ("Ok");
		return 0;
	}
}	


