using System;
using System.Collections.Generic;

public abstract class TestClass
{
	public abstract void ToString (object obj);

	public IEnumerable<object> TestEnumerator ()
	{
		ToString (null);
		yield break;
	}

	public void Test ()
	{
		ToString (null);
	}
}

class M
{
	public static void Main ()
	{
	}
}