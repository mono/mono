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
		Zub a = Zub.Foo, b = Zub.Bar;
		object v = (a + b);
	}
}
	
