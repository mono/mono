// CS0144: Cannot create an instance of the abstract class or interface `X'
// Line: 11
abstract class X {
	public abstract void B ();

}

class Demo {
	static void Main ()
	{
		object x = new X ();
	}
}
