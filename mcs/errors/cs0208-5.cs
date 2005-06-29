// cs0208-5.cs: Cannot take the address of, get the size of, or declare a pointer to a managed type `X'
// Line: 8
// Compiler options: -unsafe

unsafe struct X {
	string a;
	static void Main () {
		X* y;
	}
}

