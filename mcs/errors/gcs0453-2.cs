// CS0453: The type `Foo' must be a non-nullable value type in order to use it as parameter `T' in the generic type or method `System.Nullable<T>'
// Line: 10
public class Foo
{ }

class X
{
	static void Main ()
	{
		Foo? foo = new Foo ();
	}
}
