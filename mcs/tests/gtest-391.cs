using System;

class C
{
	bool Test_1 ()
	{
		bool? xx = null;
		return xx ?? true;
	}
	
	static void Test (object s, EventArgs a)
	{
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

		object arg = null;
		string result = arg as string ?? "";
		
		int? nint = null;
		int? r = nint ?? null;
		
		EventHandler h = new EventHandler (Test) ?? Test;
		
		return 0;
	}
}
