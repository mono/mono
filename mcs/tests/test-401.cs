// Compiler options: -unsafe

//
// This test excercises stackalloc, some pointer arithmetic,
// and dereferences
//
using System;
unsafe class X {
	public static int Main ()
	{
		char *ptr = stackalloc char [10];
	        char *cptr = ptr;
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
		if (cptr [l] != 10){
			return 1;
		}
		if (cptr [ul] != 10)
			return 2;
		if (cptr [b] != 10)
			return 3;

		//
		// Try to compile non-int values
		//
		byte* bptr = (byte*) 5;
                ushort us = 3;
                byte* ret = (byte*) (bptr + us);
			
		Console.WriteLine ("Ok");
		return 0;
	}
}	


