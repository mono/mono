// CS8139: `D.M()': cannot change return type tuple element names when overriding inherited member `C.M()'
// Line: 14

class C
{
	public virtual (int a, int b) M ()
	{
		throw null;
	}
}

class D : C
{
	public override (int c, int d) M ()
	{
		throw null;
	}
}