// CS0219: The variable `i' is assigned but its value is never used
// Line: 14
// Compiler options: -warn:3 -warnaserror

using System.Collections.Generic;

class C
{
	IEnumerable<int> Test ()
	{
		try {
			yield return 1;
		} finally {
			int i = 100;
		}
	}
}