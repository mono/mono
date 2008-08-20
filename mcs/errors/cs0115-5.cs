// CS0115: `B.Foo(ref int)' is marked as an override but no suitable method found to override
// Line: 13

class A
{
	public virtual void Foo (out int i)
	{
	}
}

class B : A
{
	public override void Foo (ref int i)
	{
	}
}
