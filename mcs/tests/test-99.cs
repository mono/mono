using System;
class X {
	enum A : int {
		a = 1, b, c
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

		return 0;
	}
}
