// CS0122: `A.AA.Foo()' is inaccessible due to its protection level
// Line: 16
// Compiler options: -r:CS0122-8-lib.dll

public class Test
{
	public static void Main ()
	{
		new B.BB ().Foo ();
	}
}
