using System;
using System.Collections;
using System.Collections.Generic;

interface ITest : IEnumerable<int> {
}

class Test : ITest {
	IEnumerator IEnumerable.GetEnumerator () { throw new Exception (); }
	IEnumerator<int> IEnumerable<int>.GetEnumerator () { yield break; }
}

class M {
	public static void Main ()
	{
		ITest foo = new Test ();
		foreach (int i in foo)
			Console.WriteLine (i);
	}
}
