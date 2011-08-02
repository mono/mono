using System;

class A
{
	static int switch1 (ulong a)
	{
		switch (a) {
		case long.MaxValue - 1:
			return 1;
		case long.MaxValue + (ulong) 1:
			return 2;
		case long.MaxValue + (ulong) 2:
			return 3;
		case long.MaxValue + (ulong) 3:
			break;
		default:
			return 4;
		}

		return 5;
	}

	static int switch2 (sbyte a)
	{
		switch (a) {
		case 0:
			return 1;
		case -1:
			return 2;
		}

		return 0;
	}

	static int switch3 (long a)
	{
		switch (a) {
		case 0:
			return 1;
		case -1:
			return 2;
		}

		return 0;
	}

	static int switch4 (ulong a)
	{
		switch (a) {
		case long.MaxValue:
			goto case ulong.MaxValue;

		case ulong.MaxValue:
			return 4;
		}

		return 0;
	}

	static int switch5(ulong x)
	{
		switch (x) {
		case 0:
			break;
		default:
			return 1;
		}

		return 2;
	}

	public static int Main ()
	{
		if (switch1 (long.MaxValue + (ulong) 1) != 2)
			return 1;

		if (switch2 (-1) != 2)
			return 2;

		if (switch3 (-1) != 2)
			return 3;

		if (switch4 (ulong.MaxValue) != 4)
			return 4;

		if (switch4 (long.MaxValue) != 4)
			return 41;

		if (switch5 (0) != 2)
			return 5;

		Console.WriteLine ("1");
		return 0;
	}
}