using System;
using System.IO;

public class Test {

	public static int boxtest ()
	{
		int i = 123;
		object o = i;
//		int j = (int) o;

//		if (i != j)
//			return 1;
		
		return 0;
	}

	public static int Main () {
		if (boxtest () != 0)
			return 1;

		
		return 0;
	}
}


