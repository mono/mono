using System;

public class Test
{
	public static void Invoke<A, TR>(Func<A, Func<TR>> callee, A arg1, TR result)
	{
	}

	static Func<int> Method (string arg)
	{
		return null;
	}

	public static void Main()
	{
		Invoke(Method, "one", 1);
	}
}

