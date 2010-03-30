using System.Linq;
using System.Collections;
using System.Collections.Generic;

class Test : IEnumerable<int>
{
	public int First {
		get {
			return 1;
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return null;
	}
	
	public IEnumerator<int> GetEnumerator ()
	{
		return null;
	}
}

class C
{
	public void Test ()
	{
		var t = new Test ();
		var v = t.First ();		
	}
	
	public static void Main ()
	{
	}
}
