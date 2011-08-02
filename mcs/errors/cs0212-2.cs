// CS0212: You can only take the address of unfixed expression inside of a fixed statement initializer
// Line: 17
// Compiler options: -unsafe

using System;

unsafe class X {
	static void Main ()
	{
		int foo = 0;
		Blah (ref foo);

	}
	
	static void Blah (ref int mptr)
	{
		int* x = &mptr;
	}
}
