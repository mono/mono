// CS0826: The type of an implicitly typed array cannot be inferred from the initializer. Try specifying array type explicitly
// Line: 8

class C
{
	public static void Main ()
	{
		var array = new[] { null, null };
	}
}