// CS8139: `D.M((int, int))': cannot change tuple element names when overriding inherited member `C.M((int, int))'
// Line: 13

class C
{
	public virtual void M ((int, int) arg)
	{
	}
}

class D : C
{
	public override void M ((int c, int d) arg)
	{
	}
}