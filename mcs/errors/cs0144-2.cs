// cs0144.cs: can not create instances of abstract classes or interfaces
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
