// CS1501: No overload for method `Foo' takes `0' arguments
// Line: 15

class Base<T>
{
	protected virtual void Foo<U> (U u)
	{
	}
}

class Derived<T> : Base<int>
{
    protected override void Foo<U> (U u)
    {
        base.Foo ();
    }
}
