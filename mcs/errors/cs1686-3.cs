// CS1686: Local variable or parameter `i' cannot have their address taken and be used inside an anonymous method or lambda expression
// Line: 18
// Compiler options: -unsafe

unsafe struct S
{
	public int i;
}

class C
{
	unsafe delegate int* D ();

	static void Main ()
	{
		unsafe {
			S str = new S ();
			D d = delegate { return &str.i; };
		}
	}
}
