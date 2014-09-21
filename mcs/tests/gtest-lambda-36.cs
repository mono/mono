using System;

class D<T>
{
	public void S<U, V> (Func<U> ftu, Func<T, U, V> ftuv)
	{
	}
}

class Test
{
	static D<V> Factory<V> (V v)
	{
		return new D<V> ();
	}

	static void Main ()
	{
		var danon = Factory (new { q = 5 });
		
		danon.S (
			() => "x",
			(l, str) => new { str }
		);
	}
}