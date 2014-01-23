using System;
using System.Collections;

struct Blah : IEnumerable {
	IEnumerator IEnumerable.GetEnumerator () {
		return new ArrayList ().GetEnumerator ();
	}
}

class B  {
	public static void Main () {
		foreach (object o in new Blah ())
			;
	}
}
