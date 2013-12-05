// CS0165: Use of unassigned local variable `a'
// Line: 16

using System;

class Program
{
	public static void Main ()
	{
		int a;
		string s = "";

		for (int i = 0; s != "s" && (a = 4) > 3; ++i) {
		}

		Console.WriteLine (a);
	}
}
