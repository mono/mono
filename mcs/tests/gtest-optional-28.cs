using System;

public class NoTypeOptionalParameters
{
	public static void Lambda (bool asc = true, params Func<string,bool>[] where)
	{
	}

	public static void MethodGroup (bool asc = true, params Func<string,bool>[] where)
	{
	}

	static bool Foo (string arg)
	{
		return false;
	}

	bool FooInstance (string arg)
	{
		return false;
	}

	public static int Main ()
	{
		bool i = false;
		Lambda (where: x => true, asc: i);
		MethodGroup (where: Foo, asc: i);
		MethodGroup (where: new NoTypeOptionalParameters ().FooInstance, asc: false);
		return 0;
	}
}
