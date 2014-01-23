public interface IFoo
{
	int GetHashCode ();
}

public interface IFoo<T>
{
	int GetHashCode ();
}

public class Test<T>
{
	public int Foo (IFoo<T> foo)
	{
		return foo.GetHashCode ();
	}

	public int Foo (IFoo foo)
	{
		return foo.GetHashCode ();
	}
}

class X
{
	public static void Main ()
	{ }
}
