// CS3006: Overloaded method `Base<T>.Test(ref int)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 13
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant (true)]

public class Base<T>
{
	public void Test (int a)
	{
	}
	public void Test (ref int b)
	{
	}
}

public class CLSClass : Base<int>
{
	public void Test ()
	{
	}
}
