// CS0023: The `?' operator cannot be applied to operand of type `T'
// Line: 13

interface IFoo<T>
{
	T Call ();
}

class C1
{
	U Foo<T, U> (IFoo<T> t)
	{
		return t?.Call ();
	}
}
