// Compiler options: -unsafe
// Test for bug #64330: A 'fixed' statement should introduce a scope

unsafe class X {
	static int x = 0;
	public static void Main () {
		fixed (void* p = &x) {}
		fixed (void* p = &x) {}
	}
}

