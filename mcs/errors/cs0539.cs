// cs0539.cs: `A.B' in explicit interface declaration is not a member of interface
// Line:

interface A {
}

class X : A {
	void A.B () {}
	static void Main () {}
}
