// CS0111: Class `Dictionary`2' already defines a member called `Add' with the same parameter types
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
