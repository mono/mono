// CS0165: Use of unassigned local variable `res'
// Line: 11

class X
{
	public static int Main ()
	{
		string[] a = null;
		int res;
		var m = a?[res = 3];
		System.Console.WriteLine (res);
		return 0;
	}
}