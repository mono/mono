// cs0677.cs: X.a volatile field can not be of type "A"
// Line: 8
using System;

struct A { int a; }

class X {
	public volatile A a;
	static void Main ()
		{
		}
}
