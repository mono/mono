using System;

class X
{
	enum Foo {
		A, B
	}

	enum Bar {
		C, D
	}

	public static void Main ()
	{
		Foo foo = Foo.A;
		Enum se = (Enum) foo;
		Enum sc = (Enum) Foo.A;
		object obj1 = (object) foo;
		object obj2 = (object) Foo.A;

		Bar bar = (Bar) se;
		Foo blah = (Foo) obj1;

		Enum Ea = Foo.A;

		IConvertible iconv = Ea;
	}
}
