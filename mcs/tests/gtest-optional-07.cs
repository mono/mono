public class Tests
{
	public static void foo (Foo f = Foo.None)
	{
	}

	public static int Main ()
	{
		foo ();
		return 0;
	}
}

public enum Foo
{
	None = 0
}