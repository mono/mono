// CS0111: A member `Dictionary<K,V>.Add(V)' is already defined. Rename this member or use different parameter types
// Line: 14

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
