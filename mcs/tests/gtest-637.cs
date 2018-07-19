using System;

public struct nint
{
	public static implicit operator nint (int v)
	{
		return 0;
	}
}

public class MainClass 
{
	static void Test (string key, int? value)
	{
	}

	static void Test (string key, nint? value)
	{
		throw new ApplicationException ();
	}

	public static void Main ()
	{
		Test (null, int.MinValue);
	}
}
