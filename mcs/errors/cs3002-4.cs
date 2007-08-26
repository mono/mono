// CS3002: Return type of `CLSClass.Foo()' is not CLS-compliant
// Line: 9

using System;
[assembly:CLSCompliant(true)]

public class CLSClass
{
	protected ulong[] Foo()
	{
		return null;
	}
}