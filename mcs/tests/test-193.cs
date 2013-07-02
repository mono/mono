using System;

class A
{
	static int Test (int i)
	{
		switch (i) {
		case 1:
			Console.WriteLine ("1");
			if (i > 0)
				goto LBL4;
			Console.WriteLine ("2");
			break;

		case 3:
			Console.WriteLine ("3");
		LBL4:
			Console.WriteLine ("4");
			return 0;
		}

		return 1;
	}

	public static int Main ()
	{
		return Test (1);
	}
}
