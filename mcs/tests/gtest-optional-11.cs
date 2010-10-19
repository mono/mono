using System;
using System.Reflection;
using System.Runtime.InteropServices;

public class C
{
	public static int TestA ([Optional][DefaultParameterValue (1)] int u)
	{
		return u;
	}

	public static T TestB<T> (T a, [Optional] T u)
	{
		return u;
	}
	
	public static object TestC ([Optional] object a)
	{
		return a;
	}

	public static int TestD ([Optional] int a, int i)
	{
		return a;
	}

	public static int Main ()
	{
		if (TestA () != 1)
			return 1;

		if (TestB (-4) != 0)
			return 2;

		if (TestB ((object) null) != Missing.Value)
			return 3;

		if (TestC () != Missing.Value)
			return 4;
		
		if (TestD (i:2) != 0)
			return 5;
		
		return 0;
	}
}
