// CS1942: An expression type in `select' clause is incorrect. Type inference failed in the call to `Select'
// Line: 18

using System;

class TestA
{
	public string Select<U> (Func<TestA, U> f)
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
