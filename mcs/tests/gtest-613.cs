class X
{
	public static void Foo (X x1)
	{
	}

	public static void Main ()
	{
		C<X, X, X>.Test (new X ());
	}

	public int Prop {
		get {
			return 5;
		}
	}
}

class Y
{
}

class C<T1, T2, T3>
	where T1 : X
	where T2 : T1
	where T3 : T2
{
	public static void Test (T3 t3)
	{
		X.Foo (t3);
		var g = t3.Prop;
	}
}