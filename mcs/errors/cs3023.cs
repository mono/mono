// cs3023.cs: CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead
// Line: 8

using System;
[assembly: CLSCompliant (true)]

public class Class {
	[return:CLSCompliant(false)]
	public ulong Test ()
	{
	    return 4;
	}
}
