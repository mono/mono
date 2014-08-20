using System;
using System.Threading.Tasks;

class Test
{
	static async Task<int> YieldValue (int a)
	{
		await Task.Yield ();
		return a;
	}

	static async Task<int> TestNestedReturn (int v)
	{
		int x = 0;

		try {
			try {
				x = await YieldValue (1);
				Console.WriteLine ("T1");
				if (x == v)
					return 6;
			} finally {
				Console.WriteLine ("F1");

				x += await YieldValue (2);
			}

			Console.WriteLine ("AF1");
		} finally {
			Console.WriteLine ("F2");
			try {
				x += await YieldValue (4);
				Console.WriteLine ("T3");
			} finally {
				Console.WriteLine ("F3");
				x += await YieldValue (8);
			}
		}

		Console.WriteLine ("END");

		return x;
	}

	static async Task<int> TestNestedGoto (int v)
	{
		int x = 0;

		try {
			try {
				Console.WriteLine ("T1");
				if (x == v)
					goto L;

				x = await YieldValue (1);
			} finally {
				Console.WriteLine ("F1");

				x += await YieldValue (2);
			}

			Console.WriteLine ("AF1");
		} finally {
			Console.WriteLine ("F2");
			try {
				x += await YieldValue (4);
				Console.WriteLine ("T3");
			} finally {
				Console.WriteLine ("F3");
				x += await YieldValue (8);
			}
		}

		Console.WriteLine ("END");
L:
		Console.WriteLine ("END L");

		return x;
	}

	public static int Main ()
	{
		if (TestNestedReturn (1).Result != 6)
			return 1;

		if (TestNestedReturn (0).Result != 15)
			return 2;

		if (TestNestedGoto (1).Result != 15)
			return 3;

		if (TestNestedGoto (0).Result != 14)
			return 4;

		return 0;
	}
}