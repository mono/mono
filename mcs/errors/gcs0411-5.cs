// CS0411: The type arguments for method `World' cannot be infered from the usage. Try specifying the type arguments explicitly.
// Line: 16
public interface IFoo<T>
{ }

public class Foo : IFoo<int>, IFoo<string>
{ }

public class Hello
{
	public void World<U> (IFoo<U> foo)
	{ }

	public void Test (Foo foo)
	{
		World (foo);
	}
}

class X
{
	static void Main ()
	{
	}
}
