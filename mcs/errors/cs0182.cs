// cs0182.cs :
// Line : 10

using System;

class My : Attribute {
	public My (object obj) { }
}

[My (null)]
class T {
	static void Main() {}
}

