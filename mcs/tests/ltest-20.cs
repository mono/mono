public class Z
{
	public Z ()
	{
		TestMethod tm = () => Test.Foo ();
	}
}

public class Test
{
	public static bool Foo ()
	{
		return true;
	}
	
	public static void Main ()
	{
	}
}

public delegate void TestMethod ();
