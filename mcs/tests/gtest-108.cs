using System;
using System.Collections.Generic;

public class Test<T>
{
	protected T item;

	public Test (T item)
	{
		this.item = item;
	}

	public IEnumerator<T> GetEnumerator()
	{
		yield return item;
	}
}

class X
{
	public static void Main ()
	{
		Test<int> test = new Test<int> (3);
		foreach (int a in test)
			;
	}
}
