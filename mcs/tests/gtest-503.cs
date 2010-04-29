using System;

public class TestAttribute : Attribute
{
	public TestAttribute (Type type)
	{
	}
}

class C<T>
{
	[Test (typeof (N<>))]
	public class N<U>
	{
	}
}

class A
{
	public static int Main ()
	{
		if (typeof (C<>.N<>).GetCustomAttributes (true).Length != 1)
			return 1;

		return 0;
	}
}