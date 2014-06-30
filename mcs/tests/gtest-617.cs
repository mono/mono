using System;
using System.Collections.Generic;
 
class Program
{
	static void Main()
	{
		foreach (string x in new B ()) {
		}
	}
}

class A
{
	public IEnumerator<string> GetEnumerator ()
	{
		var s = new List<string>();
		s.Add("1"); 
		return s.GetEnumerator();
	}
}

class B : A
{
	public IEnumerator<int> GetEnumerator (int[] x = null)
	{
		throw new NotImplementedException ();
	}
}