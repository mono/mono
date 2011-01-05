// CS1547: Keyword `void' cannot be used in this context
// Line: 10
class Foo<T>
{ }

class X
{
	static void Main ()
	{
		Foo<void> foo;
	}
}
