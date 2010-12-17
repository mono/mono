// Compiler options: -t:library

using System;
using System.Collections;
using System.Collections.Generic;

public class GlobalMonitoredCharacterCollection : ReadonlyCollection<int>
{
}

public class ReadonlyCollection<T> : IReadonlyCollection<T>
{
	protected List<T> m_items;
	protected ReadonlyCollection () { m_items = new List<T> (); }

	IEnumerator<T> IEnumerable<T>.GetEnumerator ()
	{
		return m_items.GetEnumerator ();
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return m_items.GetEnumerator ();
	}
}

public interface IReadonlyCollection<T> : IEnumerable<T>
{
}
