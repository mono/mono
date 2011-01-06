// CS0266: Cannot implicitly convert type `S?' to `A'. An explicit conversion exists (are you missing a cast?)
// Line: 9

struct S
{
	public static int Main ()
	{
		S? s = null;
		A a = s;
		return 0;
	}
}

struct A
{
	public static implicit operator A (S x)
	{
		return new A ();
	}
}
