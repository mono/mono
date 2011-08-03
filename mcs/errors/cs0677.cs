// CS0677: `X.a': A volatile field cannot be of the type `A'
// Line: 8
using System;

struct A { int a; }

class X {
	public volatile A a;
	static void Main ()
		{
		}
}
