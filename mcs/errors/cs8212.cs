// cs8212.cs: Array creation present on attribute, but array is not initialized
// Line: 14
using System;
using System.Reflection;

[AttributeUsage (AttributeTargets.All)]
public class MineAttribute : Attribute {
	public MineAttribute (Type [] t)
	{
	}
}


[Mine(new Type [2])]
public class Foo {	
	public static int Main ()
	{
		return 0;
	}
}






