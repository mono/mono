// CS0019: Operator `==' cannot be applied to operands of type `Foo' and `null'
// Line: 14

struct Foo
{
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
