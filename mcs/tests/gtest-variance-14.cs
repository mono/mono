using System;

public class A
{
}

public class B : A
{
}

public class C : A
{
	delegate void D<in T> (T t);
	delegate T D<out T, U> (U u);
	
	public static int Main ()
	{
		D<string> d_a = null;
		D<object> d_b = (D<object>) d_a;

		D<A, string> d2_a = null;
		D<B, string> d2_b = (D<B, string>) d2_a;
		D<C, string> d2_c = (D<C, string>) d2_a;

		return 0;
	}
}
