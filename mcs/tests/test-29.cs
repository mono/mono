//
// Versioning, should choose Derived.Add (1)
//
class Base {
	public int val;
	
	void Add (int x)
	{
		val = 1;
	}
}

class Derived : Base {
	void Add (double x)
	{
		val = 2;
	}
}

class Demo {

	static void Main ()
	{
		Derived d = new Derived ();

		d.Add (1);
	}
}
