using System;
using System.Threading;

class A {
	static void X () {
		Console.WriteLine ();
	}
	static void Main () {
		Thread t = new Thread (X);
	}
}






