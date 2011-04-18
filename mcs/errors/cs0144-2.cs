// CS0144: Cannot create an instance of the abstract class or interface `X'
// Line: 11
interface X {
	void A ();

}

class Demo {
	static void Main ()
	{
		object x = new X ();
	}
}
