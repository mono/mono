using System;
using System.Reflection;
using System.Runtime.InteropServices;

struct BI
{
	public static implicit operator BI (int i)
	{
		return new BI ();
	}
}

class C
{
	public static void M ([DefaultParameterValue (1 + 3)]BI step)
	{
	}
	
	public static void M2 ([DefaultParameterValue (1)] object o)
	{
	}

	public static int Main ()
	{
		var m = typeof (C).GetMethod ("M");
		var p = m.GetParameters ()[0];

		Console.WriteLine (p.Attributes);
		if (p.Attributes != ParameterAttributes.HasDefault)
			return 2;

		if ((int) p.DefaultValue != 4)
			return 1;

		Console.WriteLine (p.DefaultValue);
		return 0;
	}
}