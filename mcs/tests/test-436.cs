using foo = System.Collections;

class X : foo::IEnumerable {
	foo::IEnumerator foo::IEnumerable.GetEnumerator () { return null; }
	public static void Main ()
	{
		System.Collections.IEnumerable x = new X ();
	}
}
