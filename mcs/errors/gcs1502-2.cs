// CS1502: The best overloaded method match for `A.A(System.Collections.Generic.IList<int>[])' has some invalid arguments
// Line: 40
using System;
using System.Collections;
using System.Collections.Generic;

public struct MyStruct : IList<int>
{
	public int this [int x] { get { return 0; } set { return; } }
	public int IndexOf (int x) { return 0; }
	public void Insert (int x, int y) { return; }
	public void RemoveAt (int x) { return; }
	public int Count { get { return 0; } }
	public bool IsReadOnly { get { return false; } }
	public void Add (int x) { return; }
	public void Clear () { return; }
	public bool Contains (int x) { return false; }
	public void CopyTo (int[] x, int y) { return; }
	public bool Remove (int x) { return false; }
	public IEnumerator<int> GetEnumerator() { yield return 0; }
	IEnumerator IEnumerable.GetEnumerator() { yield return 0; }
}

public class A
{
	// This version does not compile:
	public A(IList<int>[] x) { }

	// This version compiles fine, but results in an exception:
	public A(IList<IList<int>> x) { }
}

public class Test
{
	static void Main ()
	{
		MyStruct[] myStructArray = new MyStruct[1];

		Console.WriteLine ("Trying to construct an A...");
		A a = new A (myStructArray);
		Console.WriteLine ("success!");
	}
}
