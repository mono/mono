// CS1644: Feature `exception filter' cannot be used because it is not part of the C# 5.0 language specification
// Line: 14
// Compiler options: -langversion:5

using System;

class X
{
	public static void Main ()
	{
		int x = 4;
		try {
			throw null;
		} catch (Exception) when (x > 0) {
		}
	}
}
