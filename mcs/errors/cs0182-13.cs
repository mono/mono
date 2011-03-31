// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 6

using System;

[BAttribute (null)]
public class BAttribute : Attribute
{
	public BAttribute (Attribute a)
	{
	}
	
	public static void Main ()
	{
	}
}
