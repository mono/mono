// CS0103: The name `Foo' does not exist in the current context
// Line: 17

using static S;

class S
{
	public void Foo ()
	{
	}
}

class Test
{
	public static void Main ()
	{
		Foo ();
	}
}