// cs0573.cs: can not initializer fields in structs
// Line: 5
struct A {
	int a = 1;
}

class D {
	static void Main ()
	{
		A [] a = new A [10];

	}
}
