// gcs0208.cs: Cannot take the size of an unmanaged type (X`1)
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
