using System;
using System.Collections;
using System.Collections.Generic;

public struct S : IEnumerable<int>
{
	public S (int i)
	{
	}

	public IEnumerator<int> GetEnumerator ()
	{
		return new Enumerator<int> ();
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		throw new ApplicationException ();
	}
}

public struct S2
{
	public IEnumerator<int> GetEnumerator ()
	{
		return new Enumerator<int> ();
	}
}

public struct Enumerator<T> : IEnumerator<T>
{
	public T Current {
		get {
			throw new NotImplementedException ();
		}
	}

	object IEnumerator.Current {
		get {
			throw new NotImplementedException ();
		}
	}

	public bool MoveNext ()
	{
		return false;
	}

	public void Reset ()
	{
		throw new NotImplementedException ();
	}

	public void Dispose ()
	{
		MySystem.DisposeCounter++;
	}
}

public class MySystem
{
	public static int DisposeCounter;

	public static int Main ()
	{
		S? s = new S ();
		foreach (var a in s) {
		}

		if (DisposeCounter != 1)
			return 1;

		S2? s2 = new S2 ();
		foreach (var a in s2) {
		}

		if (DisposeCounter != 2)
			return 2;

		return 0;
	}
}
