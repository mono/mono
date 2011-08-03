// CS0019: Operator `+' cannot be applied to operands of type `Test.Zub' and `Test.Zub'
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
		object v = (Zub.Foo + Zub.Foo);
	}
}
	
