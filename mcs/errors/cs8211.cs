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






