// cs0208: cannot declare a pointer to a managed type ('foo')
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
