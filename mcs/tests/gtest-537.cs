using System;

public class Test
{
	public static int Main ()
	{
		S a = new S ();
		S? b = null;
		var res = a | b;
		if (res != "op")
			return 1;
		
		var res2 = a + b;
		if (res2 != 9)
			return 2;

		return 0;
	}
}

struct S
{
	public static string operator | (S p1, S? p2)
	{ 
		return "op";
	}
	
	public static int? operator + (S p1, S? p2)
	{ 
		return 9;
	}
}
