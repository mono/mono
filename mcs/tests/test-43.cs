//
// This test is used for testing the foreach array support
//
using System;

class X {

	static int test_single (int [] a)
	{
		int total = 0;

		foreach (int i in a)
			total += i;

		return total;
	}

	static int test_continue (int [] a)
	{
		int total = 0;
		int j = 0;
		
		foreach (int i in a){
			j++;
			if (j == 5)
				continue;
			total += i;
		}

		return total;
	}

	//
	// test that we can iterate on doubles, with a float contorl
	// variable (this excercises the conversion operator and the
	// proper loading on the array
	//
	static bool test_double (double [] d)
	{
		float t = 1.0F;
		t += 2.0F + 3.0F;
		float x = 0;
		
		foreach (float f in d){
			x += f;
		}
		return x == t;
	}
	
	static int test_break (int [] a)
	{
		int total = 0;
		int j = 0;

		foreach (int i in a){
			j++;
			if (j == 5)
				break;
			total += i;
		}

		return total;
	}

	static bool test_multi (int [,] d)
	{
		int total = 0;
		
		foreach (int a in d){
			total += a;
		}
		return (total == 46);
	}
	
	public static int Main ()
	{
		int [] a = new int [10];
		int [] b = new int [2];

		for (int i = 0; i < 10; i++)
			a [i] = 10 + i;

		for (int j = 0; j < 2; j++)
			b [j] = 50 + j;

		if (test_single (a) != 145)
			return 1;

		if (test_single (b) != 101)
			return 2;

		if (test_continue (a) != 131){
			Console.WriteLine ("Expecting: 131, got " + test_continue (a));
			return 3;
		}
		
		if (test_break (a) != 46){
			Console.WriteLine ("Expecting: 46, got " + test_break (a));
			return 4;
		}
		
		double [] d = new double [] { 1.0, 2.0, 3.0 };
		if (!test_double (d))
			return 5;

		int [,] jj = new int [2,2];
		jj [0,0] = 10;
		jj [0,1] = 2;
		jj [1,0] = 30;
		jj [1,1] = 4;
		if (!test_multi (jj))
			return 6;
			
		return 0;
	}
}
	
