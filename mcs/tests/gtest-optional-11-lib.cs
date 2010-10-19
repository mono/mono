// Compiler options: -t:library

using System;
using System.Runtime.InteropServices;

public struct S
{
}

public class Lib
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
	
	public static object TestC2 (object a = null)
	{
		return a;
	}

	public static int TestD ([Optional] int a, int i)
	{
		return a;
	}

	public static void TestS (S s = default (S))
	{
	}
}
