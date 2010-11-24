// Compiler options: -r:gtest-166-lib.dll

using System;

public class Foo
{
	public static void Main () 
	{
		Comparison<TestClass.A<TestClass.Nested>> b = TestClass.B.Compare;
	}
}
