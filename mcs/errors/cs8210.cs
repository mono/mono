// Compiler options: -unsafe
// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type ('X<A>')
class X <Y> {
}

unsafe class A {

	static void Main ()
	{
		int size = sizeof (X<A>);
	}
}
