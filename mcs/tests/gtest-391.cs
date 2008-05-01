using System;

class C
{
	public static int Main ()
	{
		string a = null;
		string b = null ?? "a";
		if (b != "a")
			return 1;
		
		int? i = null ?? null;
		if (i != null)
			return 2;
		
		return 0;
	}
}