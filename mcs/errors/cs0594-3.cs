//
// cs0594: Floating-point constant is outside the range for type 'decimal|double|float'

class X {
	public static void Main() {
		float f = 1.0e999999f;
	}
}
