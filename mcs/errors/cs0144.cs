// cs0144.cs: can not create instances of abstract classes or interfaces
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
