// CS0036: An out parameter cannot have the `In' attribute
// Line: 10

using System;
using System.Runtime.InteropServices;

class ErrorCS0036 {
	int i;

	static void SetInteger ([In] out int i) {
		i = 10;
	}

	public static void Main () {
		int x;
		SetInteger (out x);
		Console.WriteLine ("The compiler should say: ErrorCS0036: {0}", x);
	}
}

