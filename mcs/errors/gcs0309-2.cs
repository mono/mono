// CS0309: The type `B' must be convertible to `I' in order to use it as parameter `T' in the generic type or method `Foo<T>'
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
