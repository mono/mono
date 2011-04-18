// CS0019: Operator `>>' cannot be applied to operands of type `E' and `int'
// Line : 10

enum E: byte {
	Min
}

class T {
	static void Main () {
		E error = E.Min >> 2;
	}
}
