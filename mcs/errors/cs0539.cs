// cs0539.cs: Member in explicit interface declaration is not a member of the interface
// Line:

interface A {
}

class X : A {
	void A.B () {}
	static void Main () {}
}
