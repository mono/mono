// CS8172: Cannot initialize a by-reference variable `j' with a value
// Line: 10

class X
{
	static int f;

	public static void Main ()
	{
		ref int j = f;
	}
}