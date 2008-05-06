using System;

class C
{
	bool Test_1 ()
	{
		bool? xx = null;
		return xx ?? true;
	}
	
	public static int Main ()
	{
		string a = null;
		string b = null ?? "a";
		if (b != "a")
			return 1;
		
		int? i = null ?? null;
		if (i != null)
			return 2;

		object z = a ?? null;
		if (i != null)
			return 3;

		string p = default (string) ?? "a";
		if (p != "a")
			return 4;
		
		string p2 = "x" ?? "a";
		if (p2 != "x")
			return 5;

		return 0;
	}
}
