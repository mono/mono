// CS0123: A method or delegate `object.ToString()' parameters do not match delegate `System.Func<string>()' parameters
// Line: 16

using System;

class Test
{
	public static void Main ()
	{
		Func<string> f = default (TypedReference).ToString;
	}
}