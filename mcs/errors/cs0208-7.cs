// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `foo'
// Line: 11
// Compiler options: -unsafe

struct foo {
	public delegate void bar (int x);
	public bar barf;
}

unsafe class t {
	static void Main () {
		foo *f = null;
	}
}
