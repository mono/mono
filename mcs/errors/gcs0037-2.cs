// CS0037: Cannot convert null to `bool' because it is a value type
// Line: 19


using System;

class TestA
{
	public string Select (Func<TestA, bool> f)
	{
		return "";
	}
}

public class C
{
	static void Main ()
	{
		string foo = from a in new TestA () select null;
	}
}
