using System;

public class Test
{
        public static int Main ()
        {
		ulong a = 0;
		bool[] r = new bool [16];

		for (int i = 1; i < 16; i++)
			r [i] = false;

		if (a < System.UInt64.MaxValue)
			r [0] = true;
		if (a <= System.UInt64.MaxValue)
			r [1] = true;
		if (System.UInt64.MaxValue > a)
			r [2] = true;
		if (System.UInt64.MaxValue >= a)
			r [3] = true;

		float b = 0F;
		if (b < System.UInt64.MaxValue)
			r [4] = true;
		if (b <= System.UInt64.MaxValue)
			r [5] = true;
		if (System.UInt64.MaxValue > b)
			r [6] = true;
		if (System.UInt64.MaxValue >= b)
			r [7] = true;

		ushort c = 0;
		if (c < System.UInt16.MaxValue)
			r [8] = true;
		if (c <= System.UInt16.MaxValue)
			r [9] = true;
		if (System.UInt16.MaxValue > c)
			r [10] = true;
		if (System.UInt16.MaxValue >= c)
			r [11] = true;

		byte d = 0;
		if (d < System.Byte.MaxValue)
			r [12] = true;
		if (d <= System.Byte.MaxValue)
			r [13] = true;
		if (System.Byte.MaxValue > d)
			r [14] = true;
		if (System.Byte.MaxValue >= d)
			r [15] = true;

		foreach (bool check in r)
			if (!check)
				return 1;

		return 0;
	}
}
