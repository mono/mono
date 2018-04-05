using System;

public class C
{
	Func<bool> f = () => Foo (out int arg);

	static bool Foo (out int arg)
	{
		arg = 2;
		return false;
	}

	public static void Main ()
	{
		new C ();
	}
}