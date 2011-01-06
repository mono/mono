// CS0310: The type `A' must have a public parameterless constructor in order to use it as parameter `T' in the generic type or method `Foo<T>'
// Line: 18

public class Foo<T>
	where T : new ()
{
}

abstract class A
{
	public A ()
	{ }
}

class X
{
	Foo<A> foo;

	static void Main ()
	{
	}
}
