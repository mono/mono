// CS0473: Explicit interface implementation `A.I<int,int>.Foo(int)' matches more than one interface member. Consider using a non-explicit implementation instead
// Line: 13
// Compiler options: -warnaserror -warn:2

interface I<T, U>
{
	void Foo (U t);
	void Foo (T u);
}

class A : I<int, int>
{
	void I<int, int>.Foo (int arg)
	{
	}

	public void Foo (int arg)
	{
	}
}
