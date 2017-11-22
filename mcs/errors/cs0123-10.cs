// CS0123: A method or delegate `object.ToString()' parameters do not match delegate `System.Func<string>()' parameters
// Line: 16
// Compiler options: -langversion:latest

using System;

public ref struct S
{
}

class Test
{
	public static void Main ()
	{
		var s = new S ();
		Func<string> f = s.ToString;
	}
}