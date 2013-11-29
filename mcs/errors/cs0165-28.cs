// CS0165: Use of unassigned local variable `a'
// Line: 13

using System;

class Program
{
	public static void Main ()
	{
		int a;
		string s = "";
		var res = s == null || ((a = 1) > 0);
		Console.WriteLine (a);
	}
}
