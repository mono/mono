// CS1686: Local variable or parameter `str' cannot have their address taken and be used inside an anonymous method, lambda expression or query expression
// Line: 11
// Compiler options: -unsafe

using System;

unsafe struct S
{
	public fixed int i [10];
}

class C
{
	static void Main ()
	{
		unsafe {
			S str;
			Func<int> e = () => str.i [3];
		}
	}
}
