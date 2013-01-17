interface I<T>
{
}

interface A<Source>
{
	void Test<U> () where U : I<Source>;
}

struct B<TContext> : A<int>
{
	public void Test<U2> () where U2 : I<int>
	{
	}
}

class C : I<int>
{
	public static void Main ()
	{
		new B<string> ().Test<C> ();
	}
}
