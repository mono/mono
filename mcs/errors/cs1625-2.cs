// CS1625: Cannot yield in the body of a finally clause
// Line: 16

using System;
using System.Collections;

class X
{
	public static IEnumerable Test (int a)
	{
		try {
			;
		} finally {
		    try {
			yield return 0;
		    }
		    finally {}
		}
        }
}
