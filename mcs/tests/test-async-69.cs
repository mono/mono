using System;
using System.Threading.Tasks;

class Test
{
	static bool fin;

	static async Task<int> YieldValue (int a)
	{
		await Task.Yield ();
		return a;
	}

	static async Task<int> TestFinallyWithReturn (int value)
	{
		fin = false;
		try {
			if (value > 4)
				return 5;

			value += 10;
			Console.WriteLine ("try");
		} finally {
			fin = true;
			Console.WriteLine ("finally");
			value += await YieldValue (100);
		}

		value += 1000;
		Console.WriteLine ("over");

		return value;
	}

	static async Task TestFinallyWithReturnNoValue (int value)
	{
		fin = false;
		try {
			if (value > 4)
				return;

			value += 10;
			Console.WriteLine ("try");
		} finally {
			fin = true;
			Console.WriteLine ("finally");
			value += await YieldValue (100);
		}

		value += 1000;
		Console.WriteLine ("over");
	}

	static async Task<int> TestFinallyWithGoto (int value)
	{
		fin = false;
		try {
			if (value > 4)
				goto L;

			value += 10;
			Console.WriteLine ("try");
		} finally {
			fin = true;
			Console.WriteLine ("finally");
			value += await YieldValue (100);
		}
		value += 1000;
L:
		Console.WriteLine ("over");
		return value;
	}

	 static async Task<int> TestFinallyWithGotoAndReturn (int value)
	{
		fin = false;
		try {
			if (value > 4)
				goto L;

			value += 10;
			Console.WriteLine ("try");
			if (value > 12)
				return 9;
		} finally {
			fin = true;
			Console.WriteLine ("finally");
			value += await YieldValue (100);
		}
		value += 1000;
L:
		Console.WriteLine ("over");
		return value;
	}

	public static int Main ()
	{
		if (TestFinallyWithReturn (9).Result != 5)
			return 1;

		if (!fin)
			return 2;

		if (TestFinallyWithReturn (1).Result != 1111)
			return 3;

		if (!fin)
			return 4;

		TestFinallyWithReturnNoValue (9).Wait ();
		if (!fin)
			return 5;

		TestFinallyWithReturnNoValue (1).Wait ();
		if (!fin)
			return 6;

		if (TestFinallyWithGoto (9).Result != 109)
			return 7;

		if (!fin)
			return 8;

		if (TestFinallyWithGoto (1).Result != 1111)
			return 9;

		if (!fin)
			return 10;

		if (TestFinallyWithGotoAndReturn (9).Result != 109)
			return 11;

		if (!fin)
			return 12;

		if (TestFinallyWithGotoAndReturn (1).Result != 1111)
			return 13;

		if (!fin)
			return 14;

		if (TestFinallyWithGotoAndReturn (3).Result != 9)
			return 15;

		if (!fin)
			return 16;

		Console.WriteLine ("ok");
		return 0;
	}
}