// cs0643.cs: 'x' duplicate named attribute argument
// Line: 8

using System;

public class A : Attribute {
	public int x;
	[A (x = 1, x = 2)]
	public static void Main ()
	{
	}
}

