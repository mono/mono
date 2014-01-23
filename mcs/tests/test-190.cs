class A
{
	private int foo = 0;

	class B : A
	{
		void Test ()
		{
			foo = 3;
		}
	}

	class C
	{
		void Test (A a)
		{
			a.foo = 4;
		}
	}

	public static void Main ()
	{ }
}
