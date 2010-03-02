// CS1540: Cannot access protected member `A.this[int]' via a qualifier of type `B'. The qualifier must be of type `C' or derived from it
// Line: 14

class A {
	protected int this [int i] { get { return i; } }
}

class B : A { }

class C : A {
	static int Main ()
	{
		B b = new B ();
		return b [0];
	}
}
