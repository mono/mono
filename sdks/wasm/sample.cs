using System;

public class Math {
	static int IntAdd (int a, int b) {
		int c = a + b;
		return c;
	}
	public static string Add (string a, string b) {
		return IntAdd (int.Parse(a), int.Parse(b)).ToString ();
	}
}
