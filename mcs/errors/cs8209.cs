// cs8209.cs: yield can not appear inside the catch clause.
// Line: 10
using System.Collections;
class X {

	IEnumerator GetEnum ()
	{
		try {
		} catch {
			yield 1;
		}
	}
	
	static void Main ()
	{
	}
}
