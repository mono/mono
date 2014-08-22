using System;

[AttributeUsage (AttributeTargets.All + 0xFFFFFE + 1)]
class A1Attribute : Attribute
{
}

[AttributeUsage ((AttributeTargets) 0xffff)]
class A2Attribute : Attribute
{
}

public class Test
{
	public static void Main ()
	{
		
	}
}