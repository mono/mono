// cs8208.cs: yield can not appear inside the finally clause.
// Line:
using System.Collections;
class X {

	IEnumerator GetEnum ()
	{
		try {
		} finally {
			yield 1;
		}
	}
	
	static void Main ()
	{
	}
}
