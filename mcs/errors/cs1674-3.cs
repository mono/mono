// CS1674: `NoIDispose': type used in a using statement must be implicitly convertible to `System.IDisposable'
// Line: 27

using System;

class MyDispose : IDisposable {
	public bool disposed;
	
	public void Dispose ()
	{
		disposed = true;
	}
}

class NoIDispose {
	static public MyDispose x;

	public NoIDispose ()
	{
	}
	
	static NoIDispose ()
	{
		x = new MyDispose ();
	}
	
	public static implicit operator MyDispose (NoIDispose a)
	{
		return x;
	}
}

class Y {
	static void B ()
	{
		using (NoIDispose a = new NoIDispose ()){
		}
	}
	
}

