// cs111.cs : Class `Blah' already contains a definition with the same return value and parameter types for method `Foo'
// Line : 10

public class Blah {

	static public void Foo (int i, int j)
	{
	}

	static public void Foo (int i, int j)
	{
	}

	public static void Main ()
	{
		int i = 1;
		int j = 2;

		Foo (i, j);
	}
}
