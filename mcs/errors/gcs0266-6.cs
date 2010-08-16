// CS0266: Cannot implicitly convert type `System.Collections.Generic.IList<int>' to `Hoge<System.Collections.Generic.IList<int>>'. An explicit conversion exists (are you missing a cast?)
// Line: 20

using System;
using System.Collections.Generic;

public class Hoge<T>
{
	public static implicit operator Hoge<T> (T value)
	{
		return null;
	}
}

public class Test
{
	static void Main ()
	{
		IList<int> x = new List<int> ();
		Hoge<IList<int>> hoge = x;
	}
}
