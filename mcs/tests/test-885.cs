using System;
using System.Reflection;

[AttributeUsage(System.AttributeTargets.All)]
class A : Attribute
{
	public double D;

	public A (double d)
	{
		this.D = d;
	}
}

class C
{
	[A (0.7f * 100.0f)]
	static int Main ()
	{
		if ((int) (0.7f * 100.0f) != 69)
			return 1;

		if ((double) (0.7f * 100.0f) != 69.9999988079071)
			return 2;

		if (!Foo (0.7f * 100.0f))
			return 3;

		A attr = (A)MethodBase.GetCurrentMethod ().GetCustomAttributes (false) [0];
		if (attr.D != 69.9999988079071)
			return 4;

		Console.WriteLine ("ok");
		return 0;
	}

	static bool Foo (double d)
	{
		return d == 69.9999988079071;
	}
}