using System;
using System.Collections;

class X
{
	public static int Main ()
	{
		foreach (var i in GetAll ()) {
		}

		return 0;
	}

	static IEnumerable GetAll ()
	{
		yield return 1;
		if (false) {
			yield return 2;
		}
	}
}
