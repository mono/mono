// CS0568: Structs cannot contain explicit parameterless constructors
// Line: 5
struct A {
	int a;
	A () { a = 1; }
}

class D {
	static void Main ()
	{
		A [] a = new A [10];

	}
}
