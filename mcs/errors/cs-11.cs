// cs-11.cs : Delegate creation expression takes only one argument
// Line : 17

public class Blah {

	public delegate int MyDelegate (int i, int j);

	public int Foo (int i, int j)
	{
		return i+j;
	}

	public static void Main ()
	{
		Blah i = new Blah ();

		MyDelegate del = new MyDelegate ();
	}
}

