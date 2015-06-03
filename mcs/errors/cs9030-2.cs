// CS9030: The left-hand side of an assignment cannot contain a null propagating operator
// Line: 15

public class Test1
{
	public class Test2
	{
		public System.EventHandler<System.EventArgs> E;
	}

	public Test2 test2 = new Test2 ();

	static void Main ()
	{
		new Test1 ()?.test2.E += null;
	}
}
