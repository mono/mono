using System;

public class Blah {

	enum Bar {
		a = MyEnum.Foo,
		b = A.c,
		c = MyEnum.Bar,
		d = myconstant
	}
	
	public enum MyEnum : byte {
		Foo = 254,
		Bar = (byte) B.y
	}

	enum A {
		a, b, c
	}
	
	enum B {
		x, y, z
	}
	
	enum AA : byte { a, b }
	enum BB : ulong { x, y }

	const int myconstant = 30;

	enum Compute { two = AA.b + B.y }
	
	// The constant assignment follows a different path		
	const Bar bar_assignment = 0;
	
	public static int Main ()
	{
		byte b = (byte) MyEnum.Foo;
		
		Console.WriteLine ("Foo has a value of " + b);

		if (b != 254)
			return 1;
		
		int i = (int) A.a;
		int j = (int) B.x;
		int k = (int) A.c;
		int l = (int) AA.b + 1;

		if ((int) Compute.two != 2)
			return 10;
		if (i != j)
			return 1;

		if (k != l)
			return 1;

		A var = A.b;

		i = (int) Bar.a;

		if (i != 254)
			return 1;

		i = (int) Bar.b;

		if (i != 2)
			return 1;

		j = (int) Bar.c;

		if (j != 1)
			return 1;

		j = (int) Bar.d;

		if (j != 30)
			return 1;

		Enum e = Bar.d;
		if (e.ToString () != "d")
			return 15;

		//
		// Test "U operator (E x, E x)"
		//
		// Notice that the Microsoft C# compiler wont compile the following
		// code, that is a bug in their compiler, see section 14.7.5 of the
		// spec.

		if ((A.c - A.a) != 2)
			return 16;

		if ((A.c - 1) != A.b)
			return 17;
		
		Console.WriteLine ("Value: " + e.ToString ());
		Console.WriteLine ("Enum emission test okay");
		return 0;
	}
}
