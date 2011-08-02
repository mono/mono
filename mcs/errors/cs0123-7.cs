// CS0123: A method or delegate `C.Method(ref dynamic)' parameters do not match delegate `C.D(dynamic)' parameters
// Line: 14

public class C
{
	delegate void D (dynamic d);
	
	static void Method (ref dynamic d)
	{
	}

	public static void Main ()
	{
		D d = Method;
	}
}

