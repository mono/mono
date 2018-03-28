// CS0815: An implicitly typed local variable declaration cannot be initialized with `(int, method group)'
// Line: 8

class XX
{
	public static void Main ()
	{
		var m = (1, Main);
	}
}