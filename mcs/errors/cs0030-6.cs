// CS0030: Cannot convert type `System.DateTime' to `string'
// Line: 10

using System;

public class Blah
{
	public static void Main ()
	{
		string s = (string)DateTime.Now;
	}
}
