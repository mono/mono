// CS0165: Use of unassigned local variable `a'
// Line: 14

using System;

class Program
{
	public static void Main ()
	{
		int a;
		string s = "";

		var res = (s == "" && (a = 4) > 3) ? 1 : a;
	}
}
