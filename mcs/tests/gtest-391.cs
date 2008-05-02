using System;

class C
{
	void Test_1 ()
	{
		bool? xx = null;
		bool brxx = xx ?? true;
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

		return 0;
	}
}
