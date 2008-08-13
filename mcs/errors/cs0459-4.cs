// CS0459: Cannot take the address of `this' because it is read-only
// Line: 11
// Compiler options: -unsafe

using System;

class X {

	unsafe void Test ()
	{
		X x = *&this;
	}
}
