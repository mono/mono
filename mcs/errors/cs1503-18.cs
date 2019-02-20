// CS1503: Argument `#1' cannot convert `int?' expression to type `System.Index'
// Line: 11

class X
{
	public static void Main ()
	{
		string x = null;
		string y = null;

		var res = x?[y?.Length];
	}
}
