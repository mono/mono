// CS0165: Use of unassigned local variable `a'
// Line: 14

using System;

class Program
{
	public static void Main ()
	{
		int a;
		string s = "";

		if (s != "s" || (a = 4) > 3) {
			Console.WriteLine (a);
		}
	}
}
