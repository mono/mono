// CS1501: No overload for method `Blah.MyDelegate' takes `0' arguments
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

