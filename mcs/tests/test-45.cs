using System;

public class Blah {

	public static int Main ()
	{
		int [] i = new int [] { 0, 1, 2, 3 };

		int [,] j = new int [,] { {0,1}, {2,3}, {4,5}, {6,7} };

		if (i [2] != 2)
			return 1;
		
		if (j [1,1] != 3)
			return 1;
		
		Console.WriteLine ("Array initialization test okay.");
				   
		return 0;
	}
}
