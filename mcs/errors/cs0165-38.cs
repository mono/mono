// CS0165: Use of unassigned local variable `a'
// Line: 16

using System;

class Program
{
	public static void Main ()
	{
		int a;
		string s = "";

		do {
		} while (s != "s" && (a = 4) > 3);

		Console.WriteLine (a);
	}
}
