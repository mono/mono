// Compiler options: -warnaserror

using System;
using System.Reflection;

class A : Attribute
{
}

[method:A]
class X (int value)
{
	public int f = value;

	public int P { get; } = value;

	public static int Main ()
	{
		var x = typeof (X);
		var attr = x.GetConstructors ()[0].GetCustomAttribute (typeof (A)) as A;
		if (attr == null)
			return 2;

		return 0;
	}
}
