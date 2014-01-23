// Compiler options: -unsafe

struct Foo {
	public float f;
	public void foo ()
	{
		unsafe {
			fixed (float *pf2 = &f) {
			}
		}
	}
}

class Test {
	public static void Main ()
	{
		Foo x = new Foo ();
		x.foo ();
	}
}
