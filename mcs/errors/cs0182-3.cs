// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
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






