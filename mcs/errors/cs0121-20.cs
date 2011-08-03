// CS0121: The call is ambiguous between the following methods or properties: `Test.Foo(string, params object[])' and `Test.Foo(string, params int[])'
// Line: 16

public class Test
{
	static void Foo (string s, params object[] args)
	{
	}
	
	static void Foo (string s, params int[] args)
	{
	}
	
	public static void Main ()
	{
		Foo ("a");
	}
}
