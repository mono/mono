using System;
using MSG = System.Collections.Generic;

public class HashSet<T>
{
	long value;

	public HashSet (long value)
	{
		this.value = value;
	}

	public long Test ()
	{
		return value;
	}

	public MSG.IEnumerator<long> GetEnumerator()
	{
		yield return Test ();
	}
}

class X
{
	public static int Main ()
	{
		HashSet<int> hb = new HashSet<int> (12345678);

		foreach (long value in hb) {
			if (value != 12345678)
				return 1;
		}

		return 0;
	}
}
