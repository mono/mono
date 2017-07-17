// CS8181: Tuple type cannot be used in an object creation expression. Use a tuple literal expression instead
// Line: 8

class C
{
	static void Main ()
	{
		var x = new (long, int) () {
			Item1 = 1
		};
	}
}
