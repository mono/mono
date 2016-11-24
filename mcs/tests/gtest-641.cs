// Compiler options: -r:gtest-641-lib.dll

using System;

class SomeClass
{
	public static void Main ()
	{
		IEquatable<Foo<int>.Bar.FooBar> a = new Foo<int>.Bar.FooBar ();
	}
}
