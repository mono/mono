// cs8206.cs: Return not allowed in iterator method
// Line: 11
using System.Collections;

class X {
	IEnumerator MyEnumerator (int a)
	{
		if (a == 0)
			yield return 1;
		else
			return 2;
	}

	static void Main ()
	{
	}
}
