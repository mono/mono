// cs0037.cs: Cannot convert null to `S' because it is a value type
// Line: 10
struct S {
	int a;
}

class X {
	static void Main ()
	{
		S s = (S) null;
	}
}
