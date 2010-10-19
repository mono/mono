// CS0122: `A.Foo()' is inaccessible due to its protection level
// Line: 23

class A
{
	public void Foo (int i)
	{
	}

	private void Foo ()
	{
	}
}

class B : A
{
	public static void Main ()
	{
	}

	void Test ()
	{
		Foo ();
	}
}