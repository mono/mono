using System;

public class Blah {

	private static int[] array = {0, 1, 2, 3};

	private static int [,] bar = { {0,1}, {4,5}, {10,20} };
	
	public static int Main ()
	{
		int [] i = new int [4] { 0, 1, 2, 3 };

		int [,] j = new int [4,2] { {0,1}, {2,3}, {4,5}, {6,7} };
		
		int [] a = { 4, 5, 6, 7 };

		int [,,] m = new int [2,3,2] {{{0,1}, {2,3}, {4,5}}, {{6,7}, {8,9}, {10,11}}};

		int foo = 1;
		int [] k = new int [] { foo, foo+1, foo+4 };

		int [,] boo = new int [,] {{foo, foo+10}, {foo+3, foo+10}};

		if (i [2] != 2)
			return 1;
		
		if (j [1,1] != 3)
			return 1;
		
		for (int t = 0; t < 4; ++t) {
			if (array [t] != t)
				return 1;
			
			if (a [t] != (t + 4))
				return 1;
		}

		if (bar [2,1] != 20)
			return 1;

		if (k [2] != 5)
			return 1;

		if (m [1,1,1] != 9)
			return 1;

		if (boo [0,1] != 11)
			return 1;
		
		Console.WriteLine ("Array initialization test okay.");
				   
		return 0;
	}
}
