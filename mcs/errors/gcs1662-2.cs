// CS1662: Cannot convert `lambda expression' to delegate type `D' because some of the return types in the block are not implicitly convertible to the delegate return type
// Line: 13


using System;

delegate int D (int i);

class X
{
	static void Main ()
	{
		D d = (int l) => Main ();
	}
}
