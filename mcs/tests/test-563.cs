// Compiler options: -warn:4 -warnaserror

using System;
using System.Collections;

public class Test
{
	internal object TestMethod (TestCollection t)
	{
		foreach (object x in t)
		{
			return x;
		}
		return null;
	}

	public static void Main ()
	{
	}
}

interface ITestCollection : IEnumerable
{
	new IEnumerator GetEnumerator ();
}

interface TestCollection : ITestCollection
{
}