public delegate int T<X> (X x);

public class B
{
	public static T<X> M<X> ()
	{
		return delegate(X x) { return 5; };
	}

	public static T<long> N ()
	{
		return delegate(long x) { return 6; };
	}
}

public class D
{
	public static void Main ()
	{
		B.M<int>();
		B.N ();
	}
}
