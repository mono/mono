using System;

class X
{
	public static int Main ()
	{
		int? a = 4;
		int? b = -a;
		Console.WriteLine (b);

		int? x = 42;
		uint y = 42;
		
		bool r = (x == y);
		if (!r)
			return 1;
			
		if (x != y)
			return 2;
			
		return 0;
	}
}
