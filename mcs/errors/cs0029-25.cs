// CS0029: Cannot implicitly convert type `Bar<long>' to `Foo<int>'
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
