// CS3023: CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead
// Line: 8
// Compiler options: -warn:1 -warnaserror

using System;
[assembly: CLSCompliant (true)]

public class Class {
	[return:CLSCompliant(false)]
	public ulong Test ()
	{
	    return 4;
	}
}
