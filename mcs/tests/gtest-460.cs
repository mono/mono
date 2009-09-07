public class Foo<T>
{
	protected class Bar<V>
	{
	}
}

public interface IBaz
{
}

public class FooImpl : Foo<IBaz>
{
	Bar<int> f;

	private class BarImpl : Bar<IBaz>
	{
	}

	public static int Main ()
	{
		new FooImpl ();
		return 0;
	}
}
