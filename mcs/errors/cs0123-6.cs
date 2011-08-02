// CS0123: A method or delegate `A<T,U>.M(T)' parameters do not match delegate `A<T,U>.D(U)' parameters
// Line: 10

class A<T, U> where T : U
{
	delegate void D (U u);

	static void M (T t)
	{
		D d = M;
	}
}
