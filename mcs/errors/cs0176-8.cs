// CS0176: Static member `string.Empty' cannot be accessed with an instance reference, qualify it with a type name instead
// Line: 9

class X
{
	public static void Main ()
	{
		string y = null;
		var x = y?.Empty;
	}
}