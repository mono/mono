// CS0212: You can only take the address of unfixed expression inside of a fixed statement initializer
// Line: 10
// Compiler options: -unsafe

struct Foo {
	public float f;
	public void foo ()
	{
		unsafe {
			float *pf1 = &f;
		}
	}
}

class Test {
	static void Main ()
	{
		Foo x = new Foo ();
		x.foo ();
	}
}
