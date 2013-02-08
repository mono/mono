using System;

public interface I<T>
{
}

public class X : I<int>
{
	public static I<TR> Test<T, TR> (I<T> source, Func<I<T>, TR> selector)
	{
		return null;
	}

	public static U First<U> (I<U> source)
	{
		return default (U);
	}

	public static void Main ()
	{
		I<int> xs = new X ();
		var left = Test (xs, First);
	}
}