// CS0128: A local variable named `x' is already defined in this scope
// Line: 8

class x {
	static int y ()
	{
		int x = 1;
		int x = 2;

		return x + x;
	}
}
