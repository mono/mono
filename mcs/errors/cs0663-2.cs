// CS0663: An overloaded method `Test.Test(out int)' cannot differ on use of parameter modifiers only
// Line: 10

public class Test
{
	public Test (ref int i)
	{
	}
	
	public Test (out int i)
	{
	}
}
