// gcs0208.cs: Cannot take the address of, get the size of, or declare a pointer to a managed type `X<A>'
// Line: 12
// Compiler options: -unsafe

class X <Y> {
}

unsafe class A {

	static void Main ()
	{
		int size = sizeof (X<A>);
	}
}
