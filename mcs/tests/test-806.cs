using System;
using System.Reflection;

class A1 : Attribute
{
	public float F;
	public float UL;
	
	public A1 (float f)
	{
		this.F = f;
	}
	
	public A1 (ulong ul)
	{
		this.UL = ul;
	}
}

[A1 (45234.567f)]
class T1
{
}

[A1 (uint.MaxValue + (ulong)1)]
class T2
{
}

class Test
{
	public static int Main ()
	{
		var A1 = typeof (T1).GetCustomAttributes (false) [0] as A1;
		if (A1.F != 45234.567f)
			return 1;

		A1 = typeof (T2).GetCustomAttributes (false) [0] as A1;
		if (A1.UL != uint.MaxValue + (ulong)1)
			return 2;
		
		return 0;
	}
}
