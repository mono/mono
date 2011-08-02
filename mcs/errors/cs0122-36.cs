// CS0122: `N.S' is inaccessible due to its protection level
// Line: 9
// Compiler options: -r:CS0122-36-lib.dll

class X
{
	static void Main ()
	{
		var v = new N.S ();
	}
}

