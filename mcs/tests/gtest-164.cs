using System;
using System.Collections;
using System.Collections.Generic;

public delegate int Int2Int (int i);

public class FunEnumerable
{
	int size;
	Int2Int f;

	public FunEnumerable(int size, Int2Int f)
	{
		this.size = size; this.f = f;
	}

	public IEnumerator<int> GetEnumerator()
	{
		yield return f (size);
	}
}

class X
{
	public static void Main ()
	{ }
}
