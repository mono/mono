// CS0122: `Y.x()' is inaccessible due to its protection level
// Line: 15
using System;

class Y {
	void x () {}

}

class X {
	static int Main ()
	{
		Y y = new Y ();

		y.x ();
		return 0;
	}
}
