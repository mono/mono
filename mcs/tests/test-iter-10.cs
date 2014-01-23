using System;
using System.Collections;

class X {
	static IEnumerator GetIt
	{
	    get {
		yield return 1;
		yield return 2;
		yield return 3;
	    }
	    set
	    {
	    }	    
	}
	
	IEnumerable this [int i]
	{
	    get {
		yield return 1*i;
		yield return 2*i;
		yield return 3*i;
	    }
	    set
	    {
	    }
	}

	public static int Main ()
	{
		IEnumerator e = GetIt;
		int total = 0;
		
		while (e.MoveNext ()){
			Console.WriteLine ("Value=" + e.Current);
			total += (int) e.Current;
		}

		if (total != 6)
			return 1;

		total = 0;
		X x = new X ();
		foreach (int i in x [2]){
			Console.WriteLine ("Value=" + i);
			total += i;
		}
		if (total != 12)
			return 2;
		
		return 0;
	}
}
