interface I
{
}

class X : I
{
}

class X2
{
	public static void Main ()
	{
		Foo<I, I> (new X[0]);
		Foo<X, I> (new X[0]);
	}

	static void Foo<T1,T2> (T2[] array) where T1 : class, T2
	{
		T1[] a = (T1[])array;
	}

	static void Foo<T1,T2> (T2[][] array) where T1 : class, T2
	{
		T1[][] a = (T1[][])array;
	}
}
