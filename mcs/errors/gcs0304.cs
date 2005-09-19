// CS0304: Cannot create an instance of the variable type 'T' because it doesn't have the new() constraint
// Line: 9

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
