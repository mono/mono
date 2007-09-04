using System;
using System.Collections.Generic;

public class Test<T>
{
	public void Invalid (T item)
	{
		Other (new T[] {item});
	}

	public void Other (IEnumerable<T> collection)
	{
	}
}

class X
{
	static void Main ()
	{ }
}
