// CS0165: Use of unassigned local variable `a'
// Line: 9

class C
{
	static void Main ()
	{
		int a;
		Foo (out a, a);
	}

	static void Foo (out int a, int b)
	{
		a = b;
	}
}