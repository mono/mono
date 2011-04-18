// CS0019: Operator `+' cannot be applied to operands of type `int' and `Test.Zub'
// Line : 11
using System;

class Test {

	enum Zub :byte {
		Foo = 99,
		Bar,
		Baz
	}
	

	static void Main ()
	{
		int v = 1;
		object foo = (v + Zub.Foo);
	}
}
	
