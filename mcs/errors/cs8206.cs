// cs8206.cs: Return not allowed in iterator method
// Line:
using System.Collections;

class X {
	IEnumerator MyEnumerator (int a)
	{
		if (a == 0)
			yield 1;
		else
			return 2;
	}

	static void Main ()
	{
	}
}
