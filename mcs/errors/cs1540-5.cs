// CS1540: Cannot access protected member `A.n' via a qualifier of type `B'. The qualifier must be of type `C.N' or derived from it
// Line: 12

class A {
	protected int n = 0;
}

class B : A { }

class C : B {
	class N {
		static internal int foo (B b) { return b.n; }
	}
	public static int Main () {
		return N.foo (new B ());
	}
}
