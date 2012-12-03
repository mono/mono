public interface IFoo<T>
{ }

public class Foo : IFoo<string>
{ }

public class Hello
{
	public void World<U> (U u, IFoo<U> foo)
	{ }

	public void World<V> (IFoo<V> foo)
	{ }

	public void Test (Foo foo)
	{
		World ("Canada", foo);
		World (foo);
	}
}

class X
{
	public static void Main ()
	{
	}
}
