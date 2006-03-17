// Bugs #77466 and #77460.
using System;
using System.Reflection;
using System.Collections.Generic;

public class Foo<T>
{
	public void Test (T t)
	{ }
}

public class Tests
{
	public static void foo<T> ()
	{
	}

	public static int Test ()
	{
		MethodInfo mi = typeof (Tests).GetMethod ("foo");
		if (!mi.IsGenericMethod)
			return 1;
		if (!mi.IsGenericMethodDefinition)
			return 2;
		MethodInfo mi2 = mi.MakeGenericMethod (new Type[] { typeof (int) });
		if (!mi2.IsGenericMethod)
			return 3;
		if (mi2.IsGenericMethodDefinition)
			return 4;

		MethodInfo mi3 = typeof (Foo<int>).GetMethod ("Test");
		if (mi3.IsGenericMethod)
			return 5;
		if (mi3.IsGenericMethodDefinition)
			return 6;

		return 0;
	}

	public static int Main ()
	{
		int result = Test ();
#if DEBUG
		if (result == 0)
			Console.WriteLine ("OK");
		else
			Console.WriteLine ("ERROR: {0}", result);
#endif
		return result;
	}
}
