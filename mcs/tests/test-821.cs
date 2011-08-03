// Compiler options: -unsafe

unsafe struct S
{
	T Test<T> ()
	{
		return default (T);
	}

	public void M ()
	{
		fixed (S* ptr = &this)
			ptr->Test<string> ();
	}
}

class A
{
	public static int Main ()
	{
		new S ().M ();
		return 0;
	}
}
