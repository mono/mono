//
// FIXED
//
interface A {
	void B ();
}

interface X {
	void B ();
}


class B : A, X {
	void X.B () {}
	void A.B () {}
	
}
