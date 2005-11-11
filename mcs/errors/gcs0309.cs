// CS0309: The type `B' must be convertible to `A' in order to use it as parameter `T' in the generic type or method `Foo<T>'
// Line: 20

public class Foo<T>
	where T : A
{
}

public class A
{
}

public class B
{
}

class X
{
	Foo<B> foo;

	static void Main ()
	{
	}
}
