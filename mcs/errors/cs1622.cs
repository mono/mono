// cs1622.cs: Return not allowed in iterator method
// Line: 11
using System.Collections;

class X {
	IEnumerator MyEnumerator (int a)
	{
		if (a == 0)
			yield return 1;
		else
			return null;
	}

	static void Main ()
	{
	}
}
