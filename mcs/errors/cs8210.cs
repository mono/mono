// Compiler options: -unsafe
// Generic type parameters
//
class X <Y> {
}

unsafe class A {

	static void Main ()
	{
		int size = sizeof (X);
	}
}
