// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 13

using System;

[AttributeUsage (AttributeTargets.All)]
public class MineAttribute : Attribute {
	public MineAttribute (Type [] t)
	{
	}
}

[Mine(new Type [(ulong) 3])]
public class Foo
{
}






