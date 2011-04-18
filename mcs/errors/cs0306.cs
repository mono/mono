// CS0306: The type `char*' may not be used as a type argument
// Line: 9
// Compiler options: -unsafe

class F<U> {}
unsafe class O {
	F<char *> f;
	static void Main () {}
}
