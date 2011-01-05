// CS031: The type `B' cannot be used as type parameter `T' in the generic type or method `Foo<T>'. There is no implicit reference conversion from `B' to `I'
// Line: 21

public class Foo<T>
	where T : A, I
{
}

public interface I
{ }

public class A
{ }

public class B : A
{ }

class X
{
	Foo<B> foo;

	static void Main ()
	{
	}
}
