// cs1502-4.cs: The best overloaded method match for `X.X(int)' has some invalid arguments
// Line: 14

public class X {

	public X (int a)
	{
	}
}

class D {
	static void Main ()
	{
		X x = new X ("hola");
	}
}

