// CS1650: Fields of static readonly field `C.s' cannot be assigned to (except in a static constructor or a variable initializer)
// Line: 14

using System;

struct S {
	public int x;
}

class C {
	static readonly S s;

	public static void Main(String[] args) {
		s.x = 42;
		Console.WriteLine(s.x);
	}
}

