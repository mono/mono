//
// This test is used for testing the foreach array support
//

class X {

	static int test_single (int [] a)
	{
		int total = 0;

		foreach (int i in a)
			total += i;

		return total;
	}

	static int Main ()
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

		return 0;
	}
}
	
