// CS0826: The type of an implicitly typed array cannot be inferred from the initializer. Try specifying array type explicitly
// Line: 8

class XX
{
	public static void Main ()
	{
		var m = new [] { (1, Main) };
	}
}