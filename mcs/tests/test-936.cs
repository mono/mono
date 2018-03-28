// Compiler options: -r:test-936-lib.dll

class X
{
	public static void Main ()
	{
		TypeWithIndexer a = new TypeWithIndexer ();
		var x = a[0];
		a[0] = x;
	}
}