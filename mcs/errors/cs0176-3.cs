// CS0176: cant access static field via instance reference. qualify w/ type name -- #59324
// Line: 12
using System;

class A {
	public static int X;
}

class T {
	static void Main () {
		A T = new A ();
		System.Console.WriteLine (T.X);
	}
}