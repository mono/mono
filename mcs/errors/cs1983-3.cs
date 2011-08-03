// CS1983: The return type of an async method must be void, Task, or Task<T>
// Line: 10
// Compiler options: -langversion:future

using System;

class C
{
	public static void Main ()
	{
		Func<string> a = async delegate { };
	}
}
