// CS8135: Tuple literal `(int, method group)' cannot be converted to type `object'
// Line: 8

class XX
{
	public static void Main ()
	{
		object m = (1, Main);
	}
}