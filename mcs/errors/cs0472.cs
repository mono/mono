// CS0472: The result of comparing `int' against null is always `true'. This operation is undocumented and it is temporary supported for compatibility reasons only
// Line: 10
// Compiler options: -warnaserror -warn:2

using System;

public class X {
	public static bool Compute (int x)
	{
		return x != null;
	}
}
