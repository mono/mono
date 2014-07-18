using System;

class R<T, U>
	where T : System.IDisposable
	where U : T
{
	public void M (U u)
	{
		using (u) {
		}
	}
}

struct S<T, U>
	where T : System.IDisposable
	where U : struct, T
{
	public void M (U u)
	{
		using (u) {
		}
	}
}

class X : IDisposable
{
	public void Dispose ()
	{
	}

	public static void Main ()
	{
		new R<X, X> ().M (new X ());
		new S<Y, Y> ().M (new Y ());
	}
}

struct Y : IDisposable
{
	public void Dispose ()
	{
	}
}