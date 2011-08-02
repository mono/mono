// CS0176: Static member `A.X' cannot be accessed with an instance reference, qualify it with a type name instead
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