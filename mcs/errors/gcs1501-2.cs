// CS1501: No overload for method `Select' takes `1' arguments
// Line: 17


using System;

class TestA
{
	public string value;
	
	public TestA (string value)
	{
		this.value = value;
	}
	
	public string Select (int i, Func<TestA, TestA> f)
	{
		return value;
	}
}

public class M
{
	static void Main ()
	{
		var v = new TestA ("Oh yes");
		string foo = from a in v select a;
	}
}
