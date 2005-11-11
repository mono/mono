// CS0452: The type `Foo' must be a reference type in order to use it as type parameter `T' in the generic type or method `MyObject<T>'
// Line: 13
public class MyObject<T>
	where T : class
{ }

struct Foo
{ }

class X
{
	MyObject<Foo> foo;

	static void Main ()
	{ }
}
