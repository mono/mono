// CS0266: Cannot implicitly convert type `A' to `B'. An explicit conversion exists (are you missing a cast?)
// Line: 17

class A
{
	public static A operator -- (A x)
	{
		return new A ();
	}
}

class B : A
{
	static void Main ()
	{
		B b = new B ();
		--b;
	}
}
