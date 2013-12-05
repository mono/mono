// CS0165: Use of unassigned local variable `a'
// Line: 13

using System;

class Program
{
	public static void Main (string[] args)
	{
		int a, b;
		string s = "";
		var res = s != null ? a = 1 : b = 2;
		Console.WriteLine (a);
	}
}
