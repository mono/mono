// CS1919: Unsafe type `int*' cannot be used in an object creation expression
// Line: 12
// Compiler options: -unsafe

using System;

public class Test
{
	static void Main ()
	{
		unsafe {
			object o = new int*();
		}
	}
}

