using System;
using System.Collections.Generic;

public class Dictionary<K,V>
{
	public void Add (V key)
	{
		throw new InvalidOperationException ();
	}

	public void Add (V value)
	{
		throw new InvalidOperationException ();
	}
}
