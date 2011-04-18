// CS0767: Cannot implement interface `I<T,U>' with the specified type parameters because it causes method `I<int,int>.Foo(ref int)' to differ on parameter modifiers only
// Line: 10

interface I<T, U>
{
    void Foo(ref U x);
    void Foo(out T x);
}

class A : I<int, int>
{
    void I<int, int>.Foo(ref int x)
	{
	}
	
    public virtual void Foo(out int x)
	{
		x = 8;
	}
}
