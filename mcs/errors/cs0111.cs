// CS0111: A member `Blah.Foo(int, int)' is already defined. Rename this member or use different parameter types
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
