using System;

struct S
{
	string value;

	public S (int arg)
	{
		throw new ApplicationException ();
	}
}

public class A
{
	public static void Main ()
	{
	}
}