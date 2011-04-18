// CS0122: `A.B' is inaccessible due to its protection level
// Line: 11

interface r {
	A.B aaa ();
}

class A {
	enum B {
		D
	}
}

class B {
	static void Main ()
	{
		A.B x = A.B.D;
	}
}
