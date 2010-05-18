// Compiler options: -warnaserror

public static class Program
{
	static void Foo (this object o)
	{
	}
	
	public static void Main ()
	{
		const object o = null;
		o.Foo ();
	}
}
