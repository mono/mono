// CS0019: Operator `==' cannot be applied to operands of type `Foo' and `null'
// Line: 14

struct Foo
{
	public static bool operator == (int d1, Foo d2)
	{
		throw new System.ApplicationException ();
	}
		
	public static bool operator != (int d1, Foo d2)
	{
		throw new System.ApplicationException ();	
	}
}

public class Test
{
	static Foo ctx;

	public static void Main ()
	{
		if (ctx == null)
			return;
	}
}
