// Compiler options: -r:gtest-552-lib.dll

class A : G<A>.GPD
{
}

public class Test
{
	public static int Main ()
	{
		var a = new A ();
		a.GT = new A ();
		a.GT.Foo ();
		return 0;
	}
}
