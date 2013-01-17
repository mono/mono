using System;
using System.Reflection;

struct S
{
}

class Program
{
	public static void Test (S s = default (S))
	{
	}

	public static int Main ()
	{
		var t = typeof (Program).GetMethod ("Test");
		var p = t.GetParameters ()[0];
		if (p.RawDefaultValue != null)
			return 1;
		
		if (p.Attributes != (ParameterAttributes.Optional | ParameterAttributes.HasDefault))
			return 2;
		
		return 0;
	}
}
