interface IFoo<out T>
{
}

struct S : IFoo<string>
{
}

public class Test
{
	static int Main ()
	{
		S s = new S ();
		IFoo<object> o = s;
		return 0;
	}
}
