// cs0114.cs: Y.XX hides inherited member X.XX.  Must use `override' or `new' keyword
// Line: 8
public abstract class X {
	public abstract void XX ();
}

class Y : X {
	void XX () {}
}
