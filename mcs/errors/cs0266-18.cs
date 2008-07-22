// CS0266: Cannot implicitly convert type `B' to `I'. An explicit conversion exists (are you missing a cast?)
// Line: 21

interface I { }

class A : I { }

class B
{
	public static explicit operator A (B from)
	{
		return new A ();
	}
}

class App
{
	public static void Main ()
	{
		B b = new B ();
		I i = b;
	}
}

