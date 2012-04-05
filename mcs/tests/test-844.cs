// Compiler options: -warn:4 -warnaserror

// Checks no CS0219 warning is issued

class C
{
	int Prop { get { return 4; } }

	int this[char arg] { get { return 2; } }

	static void Foo (ref int arg)
	{
	}

	public void Method (int i)
	{
		long p1 = Prop;
		long p2 = new C ()['h'];

		int arg = 1;
		Foo (ref arg);
	}

	public static void Main ()
	{
	}
}
