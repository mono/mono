// Compiler options: -r:gtest-552-lib.dll

class A : G<A>.GPD
{
}

class B : H<int>
{
	public class MM : M<MM>
	{
	}
}

public class Test
{
	public static int Main ()
	{
		var a = new A ();
		a.GT = new A ();
		a.GT.Foo ();
		
		new B.N<B, B.MM> ();
		return 0;
	}
}
