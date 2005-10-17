// cs0019.cs: Operator `|' cannot be applied to operands of type `int' and `E'
// Line : 10

enum E: byte {
	Min
}

class T {
	static void Main () {
		E error = E.Min | 4;
	}
}
