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
