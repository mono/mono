using System;
using System.Collections.Generic;

public class Test
{
	static void Bla<T, U> () where T : class, U
	{
		T[] ta = new T[0];
		IEnumerable<T> ita = ta;
		IEnumerable<U> itu = ita;
	}

	public static void Main ()
	{
		Bla<string, object> ();
	}
}
