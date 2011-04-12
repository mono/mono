using System;

public class Blah {

	private static int[] array = {0, 1, 2, 3};

	private static int [,] bar = { {0,1}, {4,5}, {10,20} };

	static string [] names = {
		"Miguel", "Paolo", "Dietmar", "Dick", "Ravi"
	};

	public static int Main ()
	{
		int [] i = new int [4] { 0, 1, 2, 3 };

		short [,] j = new short [4,2] { {0,1}, {2,3}, {4,5}, {6,7} };
		
		ushort [] a = { 4, 5, 6, 7 };

		long [,,] m = new long [2,3,2] {{{0,1}, {2,3}, {4,5}}, {{6,7}, {8,9}, {10,11}}};

		int foo = 1;
		int [] k = new int [] { foo, foo+1, foo+4 };

	        int [,] boo = new int [,] {{foo, foo+10}, {foo+3, foo+10}};

		float [] f_array = new float [] { 1.23F, 4.5F, 6.24F };

		double [] double_arr = new double [] { 34.4567, 90.1226, 54.9823 };

		char [] c_arr = { 'A', 'B', 'C', 'M', 'R' };

		byte [] b_arr = { 0, 3, 8, 10, 21 };

		sbyte [] s_arr = { 10, 15, 30, 123 };
		
		int[,,] ints = new int[,,] { };
		
		if (a [2] != 6)
			return 1;

		if (s_arr [3] != 123)
			return 2;
		
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

		if (f_array [0] != 1.23F)
			return 1;

		if (double_arr [1] != 90.1226)
			return 1;

		foreach (string s in names)
			Console.WriteLine ("Hello, " + s);

		if (names [0] != "Miguel")
			return 1;

		if (c_arr [4] != 'R')
			return 2;

		int count = 10;

		int [] x = new int [count];

		for (int idx = 0; idx < count; idx++)
			x [idx] = idx + 1;

		for (int idx = count; idx > 0; ){
			idx--;
			if (x [idx] != idx + 1)
				return 12;
		}

		IntPtr [] arr = { new System.IntPtr (1) };
		if (arr [0] != (IntPtr) 1)
                        return 13;

                IntPtr [] arr_i = { System.IntPtr.Zero };

                if (arr_i [0] != System.IntPtr.Zero)
                        return 14;
                
		Console.WriteLine ("Array initialization test okay.");
				   
		return 0;
	}
}
