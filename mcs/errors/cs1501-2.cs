// CS1501: No overload for method `Foo' takes `0' arguments
// Line: 20

class A
{
	protected virtual void Foo (object[] arr)
	{
	}
}

class B : A
{
	protected override void Foo (params object[] arr)
	{
	}

	static void Bar()
	{
		B b = new B ();
		b.Foo ();
	}
}
