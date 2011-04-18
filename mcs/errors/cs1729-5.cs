// CS1729: The type `C' does not contain a constructor that takes `2' arguments
// Line: 16

class C
{
	public C (int i)
	{
	}
}

public class Blah
{
	public static void Main ()
	{
		dynamic d = 1;
		var r = new C (1, d);
	}
}
