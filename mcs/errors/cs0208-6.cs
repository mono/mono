// cs0208-6.cs: Cannot take the address of a managed type ('System.Object')
// Line: 8
// Compiler options: -unsafe

unsafe struct X {
	string a;
	static void Main () {
		X x;
		void* y = &x;
	}
}

