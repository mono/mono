// CS0029: Cannot convert implicitly from `Bar`1' to `Foo`1'
// Line: 15
class Foo<T>
{
}

class Bar<T> : Foo<T>
{
}

class X
{
	static void Main ()
	{
		Foo<int> foo = new Bar<long> ();
	}
}
