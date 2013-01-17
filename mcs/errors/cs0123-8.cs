// CS0123: A method or delegate `Test.Foo(int, bool)' parameters do not match delegate `System.Func<int,bool>(int)' parameters
// Line: 15

using System;

class Test
{
	static bool Foo (int x, bool option = true)
	{
		return true;
	}

	static void Main ()
	{
		Func<int, bool> f = Foo;
	}
}
