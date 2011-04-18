// CS1622: Cannot return a value from iterators. Use the yield return statement to return a value, or yield break to end the iteration
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
