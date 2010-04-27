// CS0311: The type `B' cannot be used as type parameter `T' in the generic type or method `Foo<T>'. There is no implicit reference conversion from `B' to `A'
// Line: 19

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
