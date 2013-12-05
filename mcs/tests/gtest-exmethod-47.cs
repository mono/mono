delegate void D ();

public class C
{
	static void Main ()
	{
		S s = new S ();
		D d = s.Foo;
	}
}

public class S
{
	public void Foo (int i)
	{
	}
}

public static class Extension
{
	public static void Foo (this S s) { }
}
