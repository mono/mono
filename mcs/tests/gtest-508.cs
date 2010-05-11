using T = A<int>;

class B : T
{
	public B (int i)
		: base (i)
	{
	}

	public static void Main ()
	{
		T t = new B (4);
	}
}

class A<T> where T : struct
{
	protected A(T t)
	{
	}
}
