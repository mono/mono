// CS0457: Ambiguous user defined operators `D.implicit operator A(D)' and `C.implicit operator B(C)' when converting from `D' to `B'
// Line: 30

class A
{
}

class B : A
{
}

class C
{
	public static implicit operator B (C s)
	{
		return new B ();
	}
}

class D : C
{
	public static implicit operator A (D s)
	{
		return new B ();
	}

	public static void Main ()
	{
		D d = new D ();
		B b2 = (B) d;
	}
}
