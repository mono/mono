// CS1501: No overload for method `Foo' takes `1' arguments
// Line: 12

public class Test
{
	static void Foo (int a, int b = 1, int c = 2)
	{
	}
	
	public static void Main ()
	{
		Foo (c : -1);
	}
}

