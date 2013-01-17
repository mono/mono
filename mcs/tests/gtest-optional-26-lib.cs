// Compiler options: -t:library

using System;
using System.Runtime.CompilerServices;

public class CallerTest
{
	public static int Foo ([CallerMemberName]string arg1 = null, [CallerFilePath] string arg2 = null, [CallerLineNumberAttribute] int arg3 = -1)
	{
		if (arg1 == null)
			return 1;
		
		if (arg2 == null)
			return 2;

		if (arg3 == -1)
			return 3;
		
		return 0;
	}
}