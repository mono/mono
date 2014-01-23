// CS0165: Use of unassigned local variable `x'
// Line: 17

using System;

class Program
{
	static int Main ()
	{
		int foo = 9;
		int x;

		switch (foo) {
		case 1:
			x = 1;
			gotoTarget: 
			{
				Console.WriteLine (x);
			}
			break;
		default:
			{
				if (foo != 0) {
					goto gotoTarget;
				}

				break;
			}
		}

		return 1;
	}
}

