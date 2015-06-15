// CS0122: `Internal' is inaccessible due to its protection level
// Line: 9
// Compiler options: -r:CS0122-38-lib.dll

class M
{
	public static void Main ()
	{
		new Internal ();
	}
}