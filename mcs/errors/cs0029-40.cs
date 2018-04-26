// CS0029: Cannot implicitly convert type `S' to `System.ValueType'
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
		var s = default (S);
		ValueType s2 = s;
		var res = default (S).ToString ();
	}
}