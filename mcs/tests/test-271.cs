using System;
using System.Collections;

class T {
	struct C : IEnumerable {
		public IEnumerator GetEnumerator ()
		{
			return new ArrayList ().GetEnumerator (); 
		}
	}
	
	static C X ()
	{
		return new C ();
	}
	
	public static void Main ()
	{
		foreach (object o in X ())
			;
	}
}