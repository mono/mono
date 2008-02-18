

using System;

public class C
{
	
	delegate int di (int x);
	delegate string ds (string s);
	delegate bool db (bool x);

	static bool M (db d) { return d (false); }
	static string M (ds d) { return "b"; }
	static int M (di d) { return d (0); }
	
	public static int Main ()
	{
		string[] s = new string[] { "1" };
		int[] i = new int[] { 1 };
		
		string s_res = M (x=> M (y=> x.ToLower ()));
		int i_res = M (x=> M (y=> (x * 0) + 5));
		
		if (s_res != "b")
			return 1;
		
		if (i_res != 5)
			return 2;
		
		return 0;
	}
}
