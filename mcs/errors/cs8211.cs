// cs8211.cs: Array expression passed to attribute is not unidimensional
// Line: 14
using System;
using System.Reflection;

[AttributeUsage (AttributeTargets.All)]
public class MineAttribute : Attribute {
	public MineAttribute (Type [] t)
	{
	}
}


[Mine(new Type [2,2])]
public class Foo {	
	public static int Main ()
	{
		return 0;
	}
}






