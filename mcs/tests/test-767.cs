class Test
{
	public static int Foo ()
	{
		return 5;
	}
	
	public static void Main ()
	{
	}
}

struct S
{
	static int v = Test.Foo ();
	
	Test Test { 
		get {
			return new Test ();
		}
	}
}
