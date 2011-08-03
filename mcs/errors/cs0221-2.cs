// CS0221: Constant value `-91' cannot be converted to a `E' (use `unchecked' syntax to override)
// Line: 10

enum E:byte {
	Min = 9
}

class T {
	static void Main () {
			E error = E.Min - 100;
			System.Console.WriteLine (error);
	}
}
