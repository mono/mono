public class A
{
	public static A Get ()
	{
		return null;
	}
}

public class Test
{
	void M ()
	{
		A A = A.Get ();
	}

	public static void Main ()
	{
		var t = new Test ();
		t.M ();
	}
}