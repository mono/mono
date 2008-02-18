// CS1942: Type inference failed to infer type argument for `select' clause. Try specifying the type argument explicitly
// Line: 


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
