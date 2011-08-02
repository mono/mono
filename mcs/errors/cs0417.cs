// CS0417: `T': cannot provide arguments when creating an instance of a variable type
// Line: 9

public class Foo<T>
	where T : new ()
{
	public T Create ()
	{
		return new T (8);
	}
}

class X
{
	static void Main ()
	{
	}
}
