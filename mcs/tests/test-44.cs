//
// This test shows that the current way in which we handle blocks
// and statements is broken.  The b [q,w] code is only executed 10
// times instead of a 100
//
using System;

class X {

	static int dob (int [,]b)
	{
		int total = 0;
		
		foreach (int i in b)
			total += i;

		return total;
	}

	//
	// This tests typecasting from an object to an array of ints
	// and then doing foreach
	//
	static int count (object o)
	{
		int total = 0;

		foreach (int i in (int []) o)
			total += i;

		return total;
	}
	
	public static int Main ()
	{
		int [,] b = new int [10,10];

		for (int q = 0; q < 10; q++)
			for (int w = 0; w < 10; w++)
				b [q,w] = q * 10 + w;

		if (dob (b) != 4950)
			return 1;

		int [] a = new int [10];
		for (int i = 0; i < 10; i++)
			a [i] = 2;

		if (count (a) != 20)
			return 2;
		
		return 0;
	}
}
	
