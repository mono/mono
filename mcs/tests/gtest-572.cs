using System.Collections.Generic;

interface IC<T> : IB<T>, IEnumerable<T>
{
}

interface IB<V> : IA<V>
{
}

interface IA<W> : IEnumerable<W>
{
}

class C : IC<short>
{
	public IEnumerator<short> GetEnumerator ()
	{
		return null;
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		throw new System.NotImplementedException ();
	}

	public static void Main ()
	{
		IC<short> ic = new C ();
		var m2 = ic.GetEnumerator ();
	}
}