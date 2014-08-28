// CS0163: Control cannot fall through from one case label `case 1:' to another
// Line: 14

using System;
using System.Collections.Generic;

static class C
{
	public static IEnumerable<int> Test (int key)
	{
		switch (key) {
		case 1:
			yield return 0;
		case 2:
			yield return 2;
		default:
			throw new ArgumentOutOfRangeException ("symbol:" + key);
		}
	}
}