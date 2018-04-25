// CS0619: `A.X' is obsolete: `'
// Line: 10

using System;

public class Test
{
	public static void Main()
	{
		var m = nameof (A.X);
	}
}
	 
public class A
{
	[Obsolete ("", true)]
	public int X;
}
