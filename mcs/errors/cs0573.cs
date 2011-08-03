// CS0573: `A.a': Structs cannot have instance field initializers
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
