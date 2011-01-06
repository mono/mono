// CS0030: Cannot convert type `X' to `S'
// Line: 10

struct S {
}

class X {
	static void Main ()
	{
		S s = (S)default(X);
	}
}
