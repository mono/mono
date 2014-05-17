// CS0165: Use of unassigned local variable `a'
// Line: 14

using System;

class Program
{
	public static void Main ()
	{
		object a;
		string s = null;

		var res = s ?? (a = null);
		Console.WriteLine (a);
	}
}
