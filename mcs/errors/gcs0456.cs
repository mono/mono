// CS0456: Type parameter `U' has the `struct' constraint, so it cannot be used as a constraint for `T'
// Line: 7
using System;

class Foo<T,U>
	where T : U
	where U : struct
{ }

class X
{
	static void Main ()
	{ }
}
