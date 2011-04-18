// CS0053: Inconsistent accessibility: property type `Blah[]' is less accessible than property `A.B'
// Line: 6

public class A {

	public Blah [] B {
		get { return null; }
	}

	static void Main () {}
}

class Blah {
	
}
