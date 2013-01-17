// CS0516: Constructor `A<T>.A(T)' cannot call itself
// Line: 7

public class A<T>
{
	public A (T i)
		: this (i)
	{
	}
}
