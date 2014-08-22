// CS0472: The result of comparing value type `int' with null is always `false'
// Line: 12
// Compiler options: -warnaserror

using System;

class X
{
	public static void Main ()
	{
		int i = 0;
		var x = i == default (byte?);
	}
}
