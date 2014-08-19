// CS0165: Use of unassigned local variable `a'
// Line: 14

class C
{
	static void Main ()
	{
		bool x = true, y = true, z = true;

		int a;
		if (x ? y : (z || Foo (out a)))
			System.Console.WriteLine (z);
		else
			System.Console.WriteLine (a);
	}

	static bool Foo (out int f)
	{
		f = 1;
		return true;
	}
}