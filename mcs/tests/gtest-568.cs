using System;

class X
{
	public static int Main ()
	{
		object o = 10;
		int? x = 3;

		if ((int) o < x) {
			return 1;
		}
		
		if (x > (int) o) {
			return 2;
		}
		
		return 0;
	}
}
