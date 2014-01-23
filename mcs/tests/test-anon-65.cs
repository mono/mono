using System;

public class BaseClass
{
	public delegate void SomeDelegate ();
	public BaseClass (SomeDelegate d)
	{
		d ();
	}
}

public class TestClass : BaseClass
{
	public readonly int Result;
	public TestClass (int result)
		: base (delegate ()
	{
		Console.WriteLine (result);
	})
	{
	}

	public static int Main (string[] args)
	{
		TestClass c = new TestClass (1);
		return 0;
	}
}