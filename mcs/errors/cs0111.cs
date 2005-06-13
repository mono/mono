// cs0111.cs: Type `Blah' already defines a member called `Foo' with the same parameter types
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
