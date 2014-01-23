// Compiler options: -unsafe

struct S
{
}

class X
{
	unsafe int* Foo {
		get { return null; }
	}

	unsafe S* Foo2 {
		get { return null; }
	}

	public static void Main ()
	{
	}
}

