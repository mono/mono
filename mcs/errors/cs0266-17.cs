// CS0266:  Cannot implicitly convert type `int' to `sbyte'. An explicit conversion exists (are you missing a cast?)
// Line: 9

class S
{
	static void Main ()
	{
		sbyte s = 1;
		sbyte r = +s;
	}
}
