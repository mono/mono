// cs0182.cs : An attribute argument must be a constant expression, typeof expression or array creation expression
// Line : 10

using System;

class My : Attribute {
	public My (object obj) { }
}

[My (null)]
class T {
	static void Main() {}
}

