using System;

public struct Test
{
	public int Foo;

	public static Test Set (C c)
	{
		c.Value.Foo = 21;
		return c.Value;
	}
}

public class C
{
	public Test Value;
}
public class Driver
{
	public static int Main ()
	{
		var v = Test.Set (new C ());
		Console.WriteLine (v.Foo);
		if (v.Foo != 21)
			return 1;
		return 0;
	}
}
