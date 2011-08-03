// CS0122: `Y.Y(string)' is inaccessible due to its protection level
// Line: 12

using System;

public class Y {
	private Y(String test) {
	}
}

class T : Y {
	public T(String test, String test1) : base(test) {
	}
	static void Main () {}
}
