// Compiler options: -r:gtest-645-lib.dll

using System;
using SeparateAssembly;

class Program
{
	public static void Main()
	{
	}

	public static void AddChildButton<T1, T2>(IGenericAction<T1, T2> action)
	{
		IGenericAction<T2, T1> childAction = null;
		action.AddAction (childAction);
	}
}