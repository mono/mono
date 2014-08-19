// CS0165: Use of unassigned local variable `a'
// Line: 17

class Test
{
	public static bool Foo (out int v)
	{
		v = 0;
		return false;
	}

	static void Main()
	{
		int a;
		bool b = false;

		if ((b && Foo (out a)) || b) {
			System.Console.WriteLine (a);
		}
	}
}