// CS0266: Cannot implicitly convert type `System.Collections.Generic.IEnumerable<T>' to `System.Collections.Generic.IEnumerable<U>'. An explicit conversion exists (are you missing a cast?)
// Line: 12

using System;
using System.Collections.Generic;

public class Test
{
	static void Bla<T, U> () where T : U
	{
		IEnumerable<T> ita = null;
		IEnumerable<U> itu = ita;
	}
}
