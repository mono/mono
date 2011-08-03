// CS0619-40: `C.ob' is obsolete: `ooo'
// Line: 13

using System;

class C
{
	[Obsolete("ooo", true)]
	const int ob = 4;

	public void Test (int arg)
	{
		switch (arg) {
			case ob: return;
		}
	}
}
