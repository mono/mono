// cs0568.cs: struct can not contain an explicit parameterless constructor
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
