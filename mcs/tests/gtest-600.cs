class A { }
class B { }

interface ICharlie<T> { }

class Delta : ICharlie<A>, ICharlie<B>
{
	static void Test<U> (ICharlie<U> icu, U u)
	{
	}

	public void World<U> (U u, IFoo<U> foo)
	{
	}

	public void Test (Foo foo)
	{
		World ("Canada", foo);
	}

	static void Main ()
	{
		Test (new Delta (), new A ());
		Test (new Delta (), new B ());
	}
}

public interface IFoo<T>
{
}

public class Foo : IFoo<int>, IFoo<string>
{
}
