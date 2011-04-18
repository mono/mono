// CS0767: Cannot implement interface `I<T,U>' with the specified type parameters because it causes method `I<int,int>.Foo(ref int)' to differ on parameter modifiers only
// Line: 10

interface I<T, U>
{
    void Foo(ref U t);
    void Foo(out T u);
}

class A : I<int, int>
{
    void I<int, int>.Foo(ref int arg)
	{
	}
	
    public virtual void Foo(out int arg)
	{
		arg = 8;
	}
}
