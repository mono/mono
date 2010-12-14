public class Z { }

public class A<X, Y>
	where Y : Z
	where X : Y
{
	public X Foo (Y y)
	{
		return y as X;
	}
}

public class Foo
{
	public static int Main ()
	{
		var a = new A<Z, Z> ();
		if (a.Foo (new Z ()) == null)
			return 1;
		
		return 0;
	}
}
