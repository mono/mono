// Compiler options: -r:gtest-371-lib.dll

// Important test: Type inference

class X
{
	public static void Main ()
	{
		Test<float,int> test = new Test<float,int> ();
		test.Foo ("Hello World");
		test.Foo (new long[] { 3, 4, 5 }, 9L);
		test.Hello (3.14F, 9, test);
		test.ArrayMethod (3.14F, (float) 9 / 3);
	}
}

