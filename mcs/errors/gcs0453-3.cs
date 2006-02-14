// CS0453: The type `Bar?' must be a non-nullable value type in order to use it as type parameter `T' in the generic type or method `Foo<T>'
// Line: 14
public class Foo<T>
	where T : struct
{ }

public struct Bar
{ }

class X
{
	static void Main ()
	{
		Foo<Bar?> foo;
	}
}
