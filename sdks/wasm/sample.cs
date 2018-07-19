using System;
using System.Linq;

public class Math {
	public static int IntAdd (int a, int b) {
		int c = a + b;
		int d = c + b;
		int e = d + a;
		return e;
	}

	public int First (int[] x) {
		return x.FirstOrDefault ();
	}
}
