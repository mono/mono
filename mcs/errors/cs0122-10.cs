// CS0122: `X.Y.Y(string)' is inaccessible due to its protection level
// Line: 9
// Compiler options: -r:CS0122-10-lib.dll

using System;
using X;

class T : Y {
	public T(String test, String test1) : base(test) {
	}
	static void Main () {}
}
