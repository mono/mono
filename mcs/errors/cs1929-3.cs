// CS1929: Type `int' does not contain a member `Foo' and the best extension method overload `S.Foo(this uint)' requires an instance of type `uint'
// Line: 15

static class S
{
	public static void Foo (this uint i)
	{
	}
}

class B
{
	static void Main ()
	{
		55.Foo ();
	}
}
