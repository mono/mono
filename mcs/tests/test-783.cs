enum E { a, b }

class C
{
	static void M (E e)
	{
	}
	
	static int Test (int a)
	{
		return -1;
	}
	
	static int Test (E e)
	{
		return 1;
	}
	
	public static int Main ()
	{
		M ((uint) 0);
		M ((long) 0);
		M ((sbyte) 0);
		M ((ulong) 0);
		
		var d = E.b;
		if (Test (d - 0) != 1)
			return 1;
		
		if (Test (d - 1) != 1)
			return 2;

		if (Test (d + 0) != 1)
			return 3;

		if (Test (d + 1) != 1)
			return 4;
		
		return 0;
	}
}
