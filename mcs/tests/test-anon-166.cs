using System;
using System.Collections.Generic;

class A
{
	void Test<T, U> () where T : class, IList<U>
	{
		Action a = () => {
			var d = default (T);
			Action a2 = () => {
				d = null;
			};
			
			a2 ();
		};
		
		a ();
	}
	
	public static int Main ()
	{
		var a = new A ();
		a.Test<int[], int> ();
		return 0;
	}
}
