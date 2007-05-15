// CS0158: The label `LBL4' shadows another label by the same name in a contained scope
// Line: 17

using System;

class A
{
	static int Test(int i)
	{
		switch (i)
		{
			case 1:
				Console.WriteLine("1");
				if (i > 0)
					goto LBL4;
				Console.WriteLine("2");
				break;

			case 3:
				Console.WriteLine("3");
			LBL4:
				Console.WriteLine("4");
				return 0;
		}
	LBL4:
		Console.WriteLine("4");
		return 1;
	}
}

