// CS0695: `C<T,U>' cannot implement both `A<T>.IB' and `A<U>.IB' because they may unify for some type parameter substitutions
// Line: 11

class A<T>
{
	public interface IB
	{
	}
}

class C<T, U> : A<T>.IB, A<U>.IB
{
}