// CS0165: Use of unassigned local variable `v'
// Line: 17

using System;

class C
{
	void Test (int arg)
	{
		int v;
		switch (arg) {
			case 1:
				v = 0;
				break;
		}

		Console.WriteLine (v);
	}
}