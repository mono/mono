// Compiler options: -warnaserror

using System;
using System.Reflection;

class A : Attribute
{
}

class X ([field:A] int value)
{
	public int f = value;

	public int P {
		get {
			return value;
		}
	}

	public static int Main ()
	{
		var attr = (A)typeof (X).GetField("value", BindingFlags.NonPublic | BindingFlags.Instance).GetCustomAttribute (typeof (A));
		if (attr == null)
			return 1;

		return 0;
	}
}
