// CS0165: Use of unassigned local variable `s'
// Line: 14

using System;

class Program
{
	public static void Main ()
	{
		string s;
		object o = null;
		while (o != null || string.IsNullOrEmpty (s = (string) o.ToString ())) {
			Console.WriteLine (s);
		}
	}
}
