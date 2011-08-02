// CS0019: Operator `*' cannot be applied to operands of type `E' and `E'
// Line : 10

enum E: byte {
	Min
}

class T {
	static void Main () {
		E error = E.Min * E.Min;
	}
}
