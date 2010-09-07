public class C
{
	delegate void D (dynamic d);
	delegate void D2 (out dynamic d);
	
	static void Method (dynamic d)
	{
	}

	static void Method (dynamic d, dynamic d2)
	{
	}

	static void Method2 (dynamic d, int i)
	{
	}

	static void Method2 (out object d)
	{
		d = null;
	}

	public static void Main ()
	{
		D d = Method;
		D2 d2 = Method2;
	}
}

