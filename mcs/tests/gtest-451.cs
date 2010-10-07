// Compiler options: -r:gtest-451-lib.dll

public class Test
{
	public static int Main ()
	{
		var a = new A<int>.N1 ();
		return a.Value.Foo ();
	}
}
