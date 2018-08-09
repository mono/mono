using System;
using System.Linq;

public class Math {
	public static int IntAdd (int a, int b) {
		var cp = new Simple.Complex (10, "hello");
		int c = a + b;
		int d = c + b;
		int e = d + a;

		e += cp.DoStuff ();

		return e;
	}


	public int First (int[] x) {
		return x.FirstOrDefault ();
	}
}
