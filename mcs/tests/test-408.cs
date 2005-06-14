// Compiler options: -unsafe

unsafe class T {
	static int Main () {
		int len = 10;
		int* x = stackalloc int [len];
		return x [0];
	}
}
