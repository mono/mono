// Compiler options: /r:gtest-326-lib.dll
using System;

public class A : C5.ArrayList<int>
{ }

class X
{
	public static void Main ()
	{
		A x = new A ();
		foreach (int i in x) {
		}
	}
}
