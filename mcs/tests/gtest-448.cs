// Compiler options: -langversion:future

using System;
using System.Runtime.InteropServices;
using System.Reflection;

public class C
{
	public static void TestA ([Optional][DefaultParameterValue (1)] int u)
	{
	}

	public static void TestB (long u = 12)
	{
	}
	
	public static void TestC (decimal d = decimal.MaxValue)
	{
	}

	public static int Main ()
	{
		ParameterInfo[] info = typeof (C).GetMethod ("TestA").GetParameters ();

		if (info[0].DefaultValue.GetType () != typeof (int))
			return 1;

		if ((int) info[0].DefaultValue != 1)
			return 2;

		if (!info[0].IsOptional)
			return 3;

		info = typeof (C).GetMethod ("TestB").GetParameters ();

		if (info[0].DefaultValue.GetType () != typeof (int))
			return 11;

		if ((int) info[0].DefaultValue != 12)
			return 12;

		if (!info[0].IsOptional)
			return 13;

		info = typeof (C).GetMethod ("TestC").GetParameters ();

		if (info[0].DefaultValue.GetType () != typeof (decimal))
			return 21;

		if ((decimal) info[0].DefaultValue != decimal.MaxValue)
			return 22;

		if (!info[0].IsOptional)
			return 23;

		Console.WriteLine ("ok");
		return 0;
	}
}
