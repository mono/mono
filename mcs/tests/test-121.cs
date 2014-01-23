//
// This test excercises the fact that array evaluation in UnaryMutator and 
// CompoundAssign expressions should never mutate data more than once
//
class X {
	static int g_calls;
	
	static int g ()
	{
		g_calls++;
		return 0;
	}

	
	public static int Main ()
	{
		int [] a = new int [10];
		int i = 0;
		
		a [0] = 1;

		a [i++] += 3;

		if (i != 1)
			return 1;
		if (a [0] != 4)
			return 2;

		a [g ()]++ ;

		if (g_calls != 1)
			return 3;
		return 0;
	}
}
