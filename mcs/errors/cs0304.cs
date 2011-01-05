// CS0304: Cannot create an instance of the variable type `T' because it does not have the new() constraint
// Line: 8

public class Foo<T>
{
	public T Create ()
	{
		return new T ();
	}
}

class X
{
	static void Main ()
	{
	}
}
