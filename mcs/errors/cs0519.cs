// cs0519.cs: `System' clashes with a predefined namespace
// Line: 1

enum System { A }

class X {
	static void Main ()
	{
		System s = new System ();
	}
}
