// CS0507: `Bar.X()': cannot change access modifiers when overriding `protected internal' inherited member `Foo.X()'
// Line: 13

class Foo
{
	protected internal virtual void X ()
	{
	}
}

class Bar : Foo
{
	protected override void X ()
	{
	}
}
