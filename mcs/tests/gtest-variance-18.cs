interface IFoo<out T>
{
}

struct S : IFoo<string>
{
}

public class Test
{
	public static int Main ()
	{
		S s = new S ();
		IFoo<object> o = s;
		return 0;
	}
}
