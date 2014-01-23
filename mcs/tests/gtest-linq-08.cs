

using System;

class TestA
{
	public string value;
	
	public TestA (string value)
	{
		this.value = value;
	}
	
	public string Select<U> (Func<TestA, U> f)
	{
		return value;
	}
}

static class TestB
{
	static public TestA Where(this TestA a, Func<TestA,bool> predicate)
	{
		if (predicate (a))
			return new TestA ("where used");
		
		return null;
	}
}

public class CustomQueryExpressionPattern
{
	public static int Main ()
	{
		var v = new TestA ("Oh yes");
		string foo = from a in v select a;
		if (foo != "Oh yes")
			return 1;
		
		v = new TestA ("where?");
		
		// It also tests that select is not called in this case
		v = from a in v where a.value != "wrong" select a;
		
		if (v.value != "where used")
			return 1;
		
		Console.WriteLine (v.value.ToString ());
		return 0;
	}
}
