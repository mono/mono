using System;
using System.Threading.Tasks;

class Test
{
	static async Task<int> YieldValue (int a)
	{
		await Task.Yield ();
		return a;
	}

	public static async Task<int> BreakTest ()
	{
		int value = 0;
		try {
			for (int i = 0; i < 8; ++i) {
				try {
					try {
						value += await YieldValue (1);

						Console.WriteLine ("i = " + i);

						if (i > 2)
							break;

						if (i > 1)
							throw new ApplicationException ();
					} catch (ApplicationException) {
						Console.WriteLine ("catch");
						value += await YieldValue (100);
					}
				} finally {
					Console.WriteLine ("F1");
					value += await YieldValue (10);
				}
			}
		} finally {
			Console.WriteLine ("F2");
			value += await YieldValue (1000);
		}

		return value;
	}

	public static async Task<int> ContinueTest ()
	{
		int value = 0;
		try {
			for (int i = 0; i < 8; ++i) {
				try {
					try {
						value += await YieldValue (1);

						Console.WriteLine ("i = " + i);

						if (i < 2)
							continue;

						if (i > 1)
							throw new ApplicationException ();
					} catch (ApplicationException) {
						Console.WriteLine ("catch");
						value += await YieldValue (100);
					}
				} finally {
					Console.WriteLine ("F1");
					value += await YieldValue (10);
				}
			}
		} finally {
			Console.WriteLine ("F2");
			value += await YieldValue (1000);
		}

		return value;
	}

	public static int Main ()
	{
		if (BreakTest ().Result != 1144)
			return 1;

		if (ContinueTest ().Result != 1688)
			return 1;

		return 0;
	}
}