using System;

class X {
	static void Concat (string s1, string s2, string s3) { }
	static void Concat (params string [] ss) {
		throw new Exception ("Overload resolution failed");
	}
	public static void Main () { Concat ("a", "b", "c"); }
}
