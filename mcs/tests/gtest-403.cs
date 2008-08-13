// Compiler options: -r:gtest-403-lib.dll

using System;

interface I {}

public struct S<T> : I
{
	public void Foo ()
	{
	}
}

class T
{
	public static void Main ()
	{
		S<int> i;
		ExS<bool> e;
		i.Foo ();
		e.Bar ();
	}
}
