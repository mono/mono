//
// This program excercises invoking foreach on structures
// that implement GetEnumerator
//

using System;
using System.Collections;
struct X {
	int [] a;
	
	public IEnumerator GetEnumerator ()
	{
			a = new int [3] { 1, 2, 3};
			return a.GetEnumerator ();
	}
}

class Y {
	static X x;

	public static int Main ()
	{
		int total = 0;
		x = new X ();

		foreach (object a in x){
			total += (int) a;
		}

		if (total != 6)
			return 1;

		total = 0;

		foreach (object a in new X ()){
			total += (int) a;
		}
		if (total != 6)
			return 3;
			
		total = 0;
		
		//
		// implicit block
		//
		foreach (object a in x)
			total += (int) a;
		if (total != 6)
			return 2;
		
		return 0;
	}
}

