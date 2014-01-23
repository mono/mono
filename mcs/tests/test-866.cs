class C : B
{
	public static bool operator + (C a, short b)
	{
		return false;
	}

	public static bool operator + (C a, long b)
	{
		return false;
	}
}

class B
{
	public static bool operator + (B b, string s)
	{
		return false;
	}
}

public class Test
{
	public static void Main ()
	{
		var c = new C ();
		var a1 = c + "a";
		var a2 = c + "a";
	}
}