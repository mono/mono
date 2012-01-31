using System;

class C<T>
{
}

class A
{
	public static void Main ()
	{
		M1 ((int[][] arg) => { });
		M2 ((C<short>[] arg) => { });
		M3 ((C<short[]>[] arg) => { });
	}

	static void M1<T> (Action<T[][]> arg)
	{
	}

	static void M2<T> (Action<C<T>[]> arg)
	{
	}

	static void M3<T> (Action<C<T[]>[]> arg)
	{
	}
}
