// CS1510: A ref or out argument must be an assignable variable
// Line: 9

class C
{
	public static void Main ()
	{
		const char c = 'a';
		Foo (ref c);
	}

	static void Foo(ref char i)
	{
	}
}
