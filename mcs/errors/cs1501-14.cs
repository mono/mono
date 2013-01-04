// CS1501: No overload for method `Foo' takes `0' arguments
// Line: 15

using System;
using System.Runtime.InteropServices;

public class C
{
	public static void Foo ([DefaultParameterValue(null)] string s)
	{
	}

	public static void Main ()
	{
		Foo ();
	}
}
