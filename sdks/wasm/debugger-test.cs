using System;

public class Math { //Only append content to this class as the test suite depends on line info
	public static int IntAdd (int a, int b) {
		int c = a + b; 
		int d = c + b;
		int e = d + a;
		int f = 0;
		return e;
	}

	public static int UseComplex () {
		var complex = new Simple.Complex (10, "xx");
		var res = complex.DoStuff ();
		return res;
	}
}
