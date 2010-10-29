using System;

class Test
{
	static int Main ()
	{
		dynamic index = (uint) int.MaxValue + 1;
		dynamic array = new int[] { 1, 2 };

		try {
			var a = array [index];
			return 1;
		} catch (System.OverflowException) {
		}

		try {
			array[ulong.MaxValue] = 1;
			return 2;
		} catch (System.OverflowException) {
		}

		return 0;
	}
}