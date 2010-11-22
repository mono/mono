// Compiler options: -unsafe

public unsafe struct B {
	private fixed int a[5];
}

public unsafe class C {
	private B x;

	public void Goo() {
		fixed(B* y=&x) {
		}
	} 

	public static void Main () {}
}
