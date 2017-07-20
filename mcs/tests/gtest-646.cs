// Compiler options: -r:gtest-646-lib.dll

public class LocalBug<T>
{
	public int Foo (LocalBug<T> p1, LocalBug<T> p2)
	{
		return 1;
	}

	public int Foo (LocalBug<object> p1, LocalBug<T> p2)
	{
		return 2;
	}
}

class X
{
	public static int Main ()
	{
		var o = new CompilerBug<object> ();
		if (o.Foo (o, o) != 2)
			return 1;

		var o2 = new LocalBug<object> ();
		if (o2.Foo (o2, o2) != 2)
			return 2;

		return 0;
	}
}

