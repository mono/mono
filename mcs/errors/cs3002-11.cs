// CS3002: Return type of `CLSClass.Foo()' is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSClass
{
	protected ulong? Foo()
	{
		return 5;
	}
}
