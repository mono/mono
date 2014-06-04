// CS0181: Attribute constructor parameter has type `System.Attribute', which is not a valid attribute parameter type
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
