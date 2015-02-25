using System;
using System.Runtime.InteropServices;

public static class MainClass
{
	public static void Main (string[] args)
	{
	}

	public delegate Int32 FooDelegate ([In, Optional] int foo);
}