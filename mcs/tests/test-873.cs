using System;

class Program
{
	static int Main ()
	{
		int foo = 9;

		switch (foo) {
		case 1:
			gotoTarget: 
			{
				return 0;
			}
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

