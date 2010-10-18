using System;

public class C
{
	static int Foo (int a, int b = 1, int c = 1)
	{
		return a;
	}
	
	int v;
	int this [int a, int b = 1, int c = 2] {
		set {
			v = a * 500 + b * 50 + c;
		}
		get {
			return v;
		}
	}

	public static int Main ()
	{
		if (Foo (c: 5, a: 10) != 10)
			return 1;

		if (Foo (a: 10) != 10)
			return 2;
		
		C c = new C ();
		c [a : 1, c : 2, b : 3] = 1;
		var res = c [1];
		if (res != 652)
			return 3;
		
		return 0;
	}
}
