// CS0128: A local variable named `s' is already defined in this scope
// Line: 12

class C
{
	public static void Main ()
	{
		object o = null;

		var x1 = o is string s;
		var x2 = o is string s;
	}
}
