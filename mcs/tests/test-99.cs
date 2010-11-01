//
// Tests the resulting value of operator + (U x, E y)
// as well as implicit conversions in the above operator.
//
using System;
class X {
	enum A : int {
		a = 1, b, c
	}
	
	enum Test : short {
		A = 1,
		B
	}
	
	static int Main ()
	{
		int v = 1;
		object foo = (v + A.a);
		object foo2 = (1 + A.a);

		if (foo.GetType ().ToString () != "X+A"){
			Console.WriteLine ("Expression evaluator bug in E operator + (U x, E y)");
			return 1;
		}
		
		if (foo2.GetType ().ToString () != "X+A"){
			Console.WriteLine ("Constant folder bug in E operator + (U x, E y)");
			return 2;
		}

		// Now try the implicit conversions for underlying types in enum operators
		byte b = 1;
		short s = (short) (Test.A + b);
		
		const int e = A.b + 1 - A.a;

		//
		// Make sure that other operators still work
		if (Test.A != Test.A)
			return 3;
		if (Test.A == Test.B)
			return 4;
		
		return 0;
	}
}
