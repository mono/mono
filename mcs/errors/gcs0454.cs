// CS0454: Circular constraint dependency involving `T' and `U'
// Line: 7
using System;

class Foo<T,U>
	where T : U
	where U : T
{ }

class X
{
	static void Main ()
	{ }
}
