// cs0106: virtual, abstract or public are not valid for explicit interface implementations
// Line: 8
interface A {
	void B ();
}

class X : A {
	public virtual void A.B () {}

	static void Main () {}
}

